using Microsoft.EntityFrameworkCore;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Ecommerce.Api.DTOs;
using System.Data;

namespace Ecommerce.Api.Services;

public class OrdenService : IOrdenService
{
    private readonly AppDbContext _context;

    public OrdenService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResultadoOperacion<OrdenDto>> RealizarCheckoutAsync(string usuarioId, CheckoutDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var carrito = await _context.Carritos
            .Include(c => c.Items).ThenInclude(i => i.Producto)
            .Include(c => c.Items).ThenInclude(i => i.Variante)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (carrito is null || carrito.Items.Count == 0)
        {
            return ResultadoOperacion<OrdenDto>.Fallo("Tu carrito está vacío", TipoError.ValidacionNegocio);
        }


        foreach (var item in carrito.Items)
        {
            var stockActual = item.Variante?.Stock ?? item.Producto!.Stock;
            if (item.Cantidad > stockActual)
            {
                var detalleTalle = item.Variante is not null ? $" (talle {item.Variante.Talle})" : "";
                return ResultadoOperacion<OrdenDto>.Fallo(
                    $"\"{item.Producto!.Nombre}\"{detalleTalle} ya no tiene stock suficiente. Quedan {stockActual} unidades.",
                    TipoError.ValidacionNegocio);
            }
        }


        CuponDescuento? cupon = null;
        if (!string.IsNullOrWhiteSpace(dto.CuponCodigo))
        {
            var validacionCupon = await ValidarCuponAsync(dto.CuponCodigo);
            if (!validacionCupon.Exito)
            {
                return ResultadoOperacion<OrdenDto>.Fallo(validacionCupon.MensajeError!, TipoError.ValidacionNegocio);
            }
            cupon = validacionCupon.Datos;
        }


        if (dto.NumeroTarjeta.Replace(" ", "").EndsWith("0000"))
        {
            return ResultadoOperacion<OrdenDto>.Fallo(
                "El pago fue rechazado por la entidad emisora. Probá con otra tarjeta.",
                TipoError.ValidacionNegocio);
        }


        var subtotal = carrito.Items.Sum(i => i.Producto!.Precio * i.Cantidad);
        var porcentajeDescuento = cupon?.PorcentajeDescuento ?? 0;
        var descuento = Math.Round(subtotal * porcentajeDescuento / 100m, 2);
        var total = subtotal - descuento;

        var orden = new Orden
        {
            UsuarioId = usuarioId,
            Subtotal = subtotal,
            Descuento = descuento,
            Total = total,
            CuponCodigo = cupon?.Codigo,
            UltimosDigitosTarjeta = dto.NumeroTarjeta.Replace(" ", "")[^4..],
            Estado = EstadoOrden.Pagada
        };

        foreach (var item in carrito.Items)
        {
            orden.Items.Add(new OrdenItem
            {
                ProductoId = item.ProductoId,
                VarianteId = item.VarianteId,
                ProductoNombre = item.Producto!.Nombre,
                Talle = item.Variante?.Talle,
                PrecioUnitario = item.Producto.Precio,
                Cantidad = item.Cantidad,
                Subtotal = item.Producto.Precio * item.Cantidad
            });


            if (item.Variante is not null)
            {
                item.Variante.Stock -= item.Cantidad;
            }
            else
            {
                item.Producto.Stock -= item.Cantidad;
            }
        }

        if (cupon is not null)
        {
            cupon.VecesUsado++;
        }

        _context.Ordenes.Add(orden);
        _context.CarritoItems.RemoveRange(carrito.Items);


        await _context.SaveChangesAsync();


        var productosConVariantes = carrito.Items
            .Where(i => i.VarianteId is not null)
            .Select(i => i.ProductoId)
            .Distinct();

        foreach (var productoId in productosConVariantes)
        {
            var stockTotal = await _context.ProductoVariantes
                .Where(v => v.ProductoId == productoId)
                .SumAsync(v => v.Stock);

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto is not null) producto.Stock = stockTotal;
        }

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return ResultadoOperacion<OrdenDto>.Ok(MapearADto(orden));
    }

    public async Task<IEnumerable<OrdenDto>> ObtenerMisComprasAsync(string usuarioId)
    {
        var ordenes = await _context.Ordenes
            .Include(o => o.Items)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.Fecha)
            .ToListAsync();

        return ordenes.Select(MapearADto);
    }

    public async Task<OrdenDto?> ObtenerDetalleAsync(string usuarioId, int ordenId)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == ordenId && o.UsuarioId == usuarioId);

        return orden is null ? null : MapearADto(orden);
    }

    private async Task<ResultadoOperacion<CuponDescuento>> ValidarCuponAsync(string codigo)
    {
        var cupon = await _context.Cupones.FirstOrDefaultAsync(c => c.Codigo == codigo.Trim().ToUpper());

        if (cupon is null || !cupon.Activo)
        {
            return ResultadoOperacion<CuponDescuento>.Fallo("El cupón no existe o ya no está activo", TipoError.ValidacionNegocio);
        }

        if (cupon.FechaExpiracion.HasValue && cupon.FechaExpiracion.Value < DateTime.UtcNow)
        {
            return ResultadoOperacion<CuponDescuento>.Fallo("El cupón expiró", TipoError.ValidacionNegocio);
        }

        if (cupon.UsoMaximo.HasValue && cupon.VecesUsado >= cupon.UsoMaximo.Value)
        {
            return ResultadoOperacion<CuponDescuento>.Fallo("El cupón alcanzó su límite de usos", TipoError.ValidacionNegocio);
        }

        return ResultadoOperacion<CuponDescuento>.Ok(cupon);
    }

    public async Task<ResultadoOperacion<int>> ValidarCuponPublicoAsync(string codigo)
    {
        var resultado = await ValidarCuponAsync(codigo);
        if (!resultado.Exito)
        {
            return ResultadoOperacion<int>.Fallo(resultado.MensajeError!, TipoError.ValidacionNegocio);
        }
        return ResultadoOperacion<int>.Ok(resultado.Datos!.PorcentajeDescuento);
    }

    private static OrdenDto MapearADto(Orden o)
    {
        return new OrdenDto
        {
            Id = o.Id,
            Fecha = o.Fecha,
            Estado = o.Estado.ToString(),
            Subtotal = o.Subtotal,
            Descuento = o.Descuento,
            Total = o.Total,
            CuponCodigo = o.CuponCodigo,
            UltimosDigitosTarjeta = o.UltimosDigitosTarjeta,
            Items = o.Items.Select(i => new OrdenItemDto
            {
                ProductoId = i.ProductoId,
                ProductoNombre = i.ProductoNombre,
                Talle = i.Talle,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Subtotal = i.Subtotal
            }).ToList()
        };
    }
}
