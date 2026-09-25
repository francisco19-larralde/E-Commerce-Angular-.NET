using Microsoft.EntityFrameworkCore;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Services.Interfaces;

namespace Ecommerce.Api.Services;

public class ProductoService : IProductoService
{
    private readonly AppDbContext _context;

    public ProductoService(AppDbContext context)
    {
        _context = context;
    }

    // Público: solo productos activos (vidriera de la tienda)
    public async Task<IEnumerable<ProductoDto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Variantes)
            .Where(p => p.Activo)
            .Select(p => MapearADto(p))
            .ToListAsync();
    }
    public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Variantes)
            .FirstOrDefaultAsync(p => p.Id == id);

        return producto is null ? null : MapearADto(producto);
    }

    // Admin: todos los productos (activos e inactivos), paginados y filtrables
    public async Task<PaginacionDto<ProductoDto>> ObtenerPaginadoAsync(
        int pagina, int tamanioPagina, int? categoriaId, string? busqueda)
    {
        var query = _context.Productos.Include(p => p.Categoria).Include(p => p.Variantes).AsQueryable();

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p => p.Nombre.Contains(busqueda));
        }

        var totalRegistros = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(p => MapearADto(p))
            .ToListAsync();

        return new PaginacionDto<ProductoDto>
        {
            Items = items,
            PaginaActual = pagina,
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanioPagina),
            TotalRegistros = totalRegistros
        };
    }

    public async Task<ResultadoOperacion<ProductoDto>> CrearAsync(CrearProductoDto dto)
    {
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExiste)
        {
            return ResultadoOperacion<ProductoDto>.Fallo(
                "La categoría indicada no existe", TipoError.ValidacionNegocio);
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl,
            Destacado = dto.Destacado,
            CategoriaId = dto.CategoriaId
        };

        if (!producto.TieneVariantes)
        {
            producto.Stock = dto.Stock;
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return ResultadoOperacion<ProductoDto>.Ok(MapearADto(producto));
    }

    public async Task<ResultadoOperacion> ActualizarAsync(int id, CrearProductoDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
        {
            return ResultadoOperacion.Fallo(
                $"No se encontró el producto con id {id}", TipoError.NoEncontrado);
        }

        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExiste)
        {
            return ResultadoOperacion.Fallo(
                "La categoría indicada no existe", TipoError.ValidacionNegocio);
        }

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.Destacado = dto.Destacado;
        producto.CategoriaId = dto.CategoriaId;

        if (!producto.TieneVariantes)
        {
            producto.Stock = dto.Stock;
        }

        await _context.SaveChangesAsync();
        return ResultadoOperacion.Ok();
    }

    // Actualización parcial: solo toca los campos que vengan con valor
    public async Task<ResultadoOperacion<ProductoDto>> ActualizarEstadoAsync(int id, ActualizarEstadoProductoDto dto)
    {
        var producto = await _context.Productos.Include(p => p.Variantes).Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return ResultadoOperacion<ProductoDto>.Fallo(
                $"No se encontró el producto con id {id}", TipoError.NoEncontrado);
        }

        if (dto.Destacado.HasValue) producto.Destacado = dto.Destacado.Value;
        if (dto.Activo.HasValue) producto.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();
        return ResultadoOperacion<ProductoDto>.Ok(MapearADto(producto));
    }

    public async Task<ResultadoOperacion> EliminarAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
        {
            return ResultadoOperacion.Fallo(
                $"No se encontró el producto con id {id}", TipoError.NoEncontrado);
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Ok();
    }

    public async Task<IEnumerable<ProductoDto>> BuscarAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Trim().Length < 2)
        {
            return [];
        }

        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Nombre.Contains(termino.Trim()))
            .OrderBy(p => p.Nombre)
            .Take(8)
            .Select(p => MapearADto(p))
            .ToListAsync();
    }

    private static ProductoDto MapearADto(Producto p)
    {
        return new ProductoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            ImagenUrl = p.ImagenUrl,
            Destacado = p.Destacado,
            Activo = p.Activo,
            TieneVariantes = p.TieneVariantes,
            Variantes = p.Variantes
                .OrderBy(v => v.Orden)
                .Select(v => new VarianteDto { Id = v.Id, Talle = v.Talle, Stock = v.Stock, Orden = v.Orden })
                .ToList(),
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null
        };
    }

    public async Task<PaginacionDto<ProductoDto>> ObtenerCatalogoAsync(FiltroCatalogoDto filtro)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Variantes)
            .Where(p => p.Activo)
            .AsQueryable();

        if (filtro.CategoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        }

        if (filtro.PrecioMin.HasValue)
        {
            query = query.Where(p => p.Precio >= filtro.PrecioMin.Value);
        }

        if (filtro.PrecioMax.HasValue)
        {
            query = query.Where(p => p.Precio <= filtro.PrecioMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Talle))
        {

            query = query.Where(p => p.Variantes.Any(v => v.Talle == filtro.Talle && v.Stock > 0));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            query = query.Where(p => p.Nombre.Contains(filtro.Busqueda.Trim()));
        }

        query = filtro.OrdenarPor switch
        {
            "precio_asc" => query.OrderBy(p => p.Precio),
            "precio_desc" => query.OrderByDescending(p => p.Precio),
            "nombre" => query.OrderBy(p => p.Nombre),
            _ => query.OrderByDescending(p => p.FechaCreacion)
        };

        var totalRegistros = await query.CountAsync();

        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanioPagina)
            .Take(filtro.TamanioPagina)
            .Select(p => MapearADto(p))
            .ToListAsync();

        return new PaginacionDto<ProductoDto>
        {
            Items = items,
            PaginaActual = filtro.Pagina,
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.TamanioPagina),
            TotalRegistros = totalRegistros
        };
    }

    public async Task<IEnumerable<string>> ObtenerTallesDisponiblesAsync()
    {
        return await _context.ProductoVariantes
            .Where(v => v.Stock > 0 && v.Producto!.Activo)
            .Select(v => v.Talle)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }
}
