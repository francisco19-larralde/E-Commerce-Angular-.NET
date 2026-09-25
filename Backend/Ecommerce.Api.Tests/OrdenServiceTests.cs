using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Api.Tests;

public sealed class OrdenServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private AppDbContext _context = null!;
    private OrdenService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _service = new OrdenService(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Checkout_con_carrito_vacio_devuelve_error()
    {
        var resultado = await _service.RealizarCheckoutAsync("usuario-1", CheckoutValido());

        Assert.False(resultado.Exito);
        Assert.Equal(TipoError.ValidacionNegocio, resultado.TipoError);
        Assert.Empty(await _context.Ordenes.ToListAsync());
    }

    [Fact]
    public async Task Checkout_con_stock_insuficiente_no_modifica_datos()
    {
        var (_, producto, carrito) = await CrearCarritoAsync(stock: 1, cantidad: 2);

        var resultado = await _service.RealizarCheckoutAsync(carrito.UsuarioId, CheckoutValido());

        Assert.False(resultado.Exito);
        Assert.Equal(1, producto.Stock);
        Assert.Single(await _context.CarritoItems.ToListAsync());
        Assert.Empty(await _context.Ordenes.ToListAsync());
    }

    [Fact]
    public async Task Checkout_exitoso_crea_orden_descuenta_stock_y_vacia_carrito()
    {
        var (_, producto, carrito) = await CrearCarritoAsync(stock: 5, cantidad: 2, precio: 1250m);
        _context.Cupones.Add(new CuponDescuento
        {
            Codigo = "AHORRO10",
            PorcentajeDescuento = 10,
            Activo = true
        });
        await _context.SaveChangesAsync();

        var checkout = CheckoutValido();
        checkout.CuponCodigo = " ahorro10 ";
        var resultado = await _service.RealizarCheckoutAsync(carrito.UsuarioId, checkout);

        Assert.True(resultado.Exito);
        Assert.NotNull(resultado.Datos);
        Assert.Equal(2500m, resultado.Datos.Subtotal);
        Assert.Equal(250m, resultado.Datos.Descuento);
        Assert.Equal(2250m, resultado.Datos.Total);
        Assert.Equal("1111", resultado.Datos.UltimosDigitosTarjeta);
        Assert.Equal(3, producto.Stock);
        Assert.Empty(await _context.CarritoItems.ToListAsync());
        Assert.Single(await _context.Ordenes.ToListAsync());
        Assert.Equal(1, await _context.Cupones.Select(c => c.VecesUsado).SingleAsync());
    }

    [Fact]
    public async Task Detalle_de_orden_no_se_expone_a_otro_usuario()
    {
        var usuario = CrearUsuario("propietario");
        var orden = new Orden
        {
            UsuarioId = usuario.Id,
            Usuario = usuario,
            Subtotal = 100m,
            Total = 100m
        };
        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();

        var detalleAjeno = await _service.ObtenerDetalleAsync("otro-usuario", orden.Id);
        var detallePropio = await _service.ObtenerDetalleAsync("propietario", orden.Id);

        Assert.Null(detalleAjeno);
        Assert.NotNull(detallePropio);
    }

    private async Task<(Categoria Categoria, Producto Producto, Carrito Carrito)> CrearCarritoAsync(
        int stock,
        int cantidad,
        decimal precio = 100m)
    {
        var usuario = CrearUsuario("usuario-1");
        var categoria = new Categoria { Nombre = "Prueba" };
        var producto = new Producto
        {
            Nombre = "Producto de prueba",
            Categoria = categoria,
            Precio = precio,
            Stock = stock
        };
        var carrito = new Carrito
        {
            UsuarioId = usuario.Id,
            Usuario = usuario,
            Items =
            [
                new CarritoItem
                {
                    Producto = producto,
                    Cantidad = cantidad
                }
            ]
        };

        _context.Carritos.Add(carrito);
        await _context.SaveChangesAsync();
        return (categoria, producto, carrito);
    }

    private static ApplicationUser CrearUsuario(string id) => new()
    {
        Id = id,
        UserName = $"{id}@example.test",
        NormalizedUserName = $"{id.ToUpperInvariant()}@EXAMPLE.TEST",
        Email = $"{id}@example.test",
        NormalizedEmail = $"{id.ToUpperInvariant()}@EXAMPLE.TEST",
        Nombre = "Usuario",
        Apellido = "Prueba",
        SecurityStamp = Guid.NewGuid().ToString()
    };

    private static CheckoutDto CheckoutValido() => new()
    {
        NumeroTarjeta = "4111 1111 1111 1111",
        NombreTitular = "Usuario Prueba",
        Vencimiento = "12/30",
        Cvv = "123"
    };
}
