using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Api.Tests;

public sealed class ImagenServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private AppDbContext _context = null!;
    private AlmacenamientoFalso _almacenamiento = null!;
    private ImagenService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _almacenamiento = new AlmacenamientoFalso();
        _service = new ImagenService(_context, _almacenamiento);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Subir_imagen_delega_el_archivo_y_persiste_la_url()
    {
        var producto = await CrearProductoAsync();
        var contenido = new MemoryStream([1, 2, 3]);
        var archivo = new FormFile(contenido, 0, contenido.Length, "archivo", "foto.jpg");

        var resultado = await _service.SubirImagenAsync(producto.Id, archivo, "https://api.example.test");

        Assert.True(resultado.Exito);
        Assert.Equal(_almacenamiento.UrlGenerada, producto.ImagenUrl);
        Assert.Equal(".jpg", _almacenamiento.ExtensionGuardada);
        Assert.Equal($"{producto.Nombre}-{producto.Id}", _almacenamiento.NombreArchivoGuardado);
    }

    [Fact]
    public async Task Reemplazar_imagen_elimina_la_anterior_despues_de_guardar()
    {
        var producto = await CrearProductoAsync("https://api.example.test/uploads/productos/anterior.jpg");
        var contenido = new MemoryStream([1, 2, 3]);
        var archivo = new FormFile(contenido, 0, contenido.Length, "archivo", "nueva.webp");

        await _service.SubirImagenAsync(producto.Id, archivo, "https://api.example.test");

        Assert.Contains("https://api.example.test/uploads/productos/anterior.jpg", _almacenamiento.Eliminadas);
    }

    [Fact]
    public async Task Reemplazar_imagen_en_la_misma_url_no_elimina_el_archivo_nuevo()
    {
        const string imagenUrl = "https://api.example.test/uploads/productos/producto-1.jpg";
        var producto = await CrearProductoAsync(imagenUrl);
        _almacenamiento.UrlGenerada = imagenUrl;
        var contenido = new MemoryStream([1, 2, 3]);
        var archivo = new FormFile(contenido, 0, contenido.Length, "archivo", "nueva.jpg");

        await _service.SubirImagenAsync(producto.Id, archivo, "https://api.example.test");

        Assert.DoesNotContain(imagenUrl, _almacenamiento.Eliminadas);
    }

    [Fact]
    public async Task Actualizar_producto_no_borra_la_imagen_persistida()
    {
        const string imagenUrl = "https://api.example.test/uploads/productos/producto-1.jpg";
        var producto = await CrearProductoAsync(imagenUrl);
        var service = new ProductoService(_context);
        var dto = new CrearProductoDto
        {
            Nombre = "Producto actualizado",
            Descripcion = "Descripción actualizada",
            Precio = 250m,
            Stock = 3,
            ImagenUrl = null,
            CategoriaId = producto.CategoriaId
        };

        var resultado = await service.ActualizarAsync(producto.Id, dto);

        Assert.True(resultado.Exito);
        Assert.Equal(imagenUrl, producto.ImagenUrl);
    }

    private async Task<Producto> CrearProductoAsync(string? imagenUrl = null)
    {
        var producto = new Producto
        {
            Nombre = $"Producto {Guid.NewGuid()}",
            Categoria = new Categoria { Nombre = $"Categoría {Guid.NewGuid()}" },
            Precio = 100m,
            Stock = 1,
            ImagenUrl = imagenUrl
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    private sealed class AlmacenamientoFalso : IAlmacenamientoImagenes
    {
        public string UrlGenerada { get; set; } = "https://cdn.example.test/nueva.jpg";
        public string? ExtensionGuardada { get; private set; }
        public string? NombreArchivoGuardado { get; private set; }
        public List<string> Eliminadas { get; } = [];

        public Task<string> GuardarAsync(
            IFormFile archivo,
            string extension,
            string nombreArchivo,
            string urlBase)
        {
            ExtensionGuardada = extension;
            NombreArchivoGuardado = nombreArchivo;
            return Task.FromResult(UrlGenerada);
        }

        public Task EliminarAsync(string? urlImagen)
        {
            if (urlImagen is not null) Eliminadas.Add(urlImagen);
            return Task.CompletedTask;
        }
    }
}
