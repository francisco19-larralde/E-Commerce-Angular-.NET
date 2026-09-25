using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Api.Tests;

public sealed class AppDbContextTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task No_permite_codigos_de_cupon_duplicados()
    {
        _context.Cupones.AddRange(
            new CuponDescuento { Codigo = "UNICO", PorcentajeDescuento = 10 },
            new CuponDescuento { Codigo = "UNICO", PorcentajeDescuento = 20 });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task No_permite_stock_negativo()
    {
        var categoria = new Categoria { Nombre = "Categoría" };
        _context.Productos.Add(new Producto
        {
            Nombre = "Producto inválido",
            Categoria = categoria,
            Precio = 100m,
            Stock = -1
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task No_permite_dos_carritos_para_el_mismo_usuario()
    {
        var usuario = new ApplicationUser
        {
            Id = "usuario-unico",
            UserName = "usuario@example.test",
            Nombre = "Usuario",
            Apellido = "Prueba"
        };
        _context.Users.Add(usuario);
        _context.Carritos.AddRange(
            new Carrito { UsuarioId = usuario.Id },
            new Carrito { UsuarioId = usuario.Id });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }
}
