using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Ecommerce.Api.Tests;

public sealed class CompraEndToEndTests : IClassFixture<EcommerceApiFactory>
{
    private readonly EcommerceApiFactory _factory;

    public CompraEndToEndTests(EcommerceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registro_login_carrito_checkout_e_historial_funcionan_de_extremo_a_extremo()
    {
        using var client = _factory.CreateClient();
        var email = $"cliente-{Guid.NewGuid():N}@example.test";
        var password = "Segura1!";

        var registro = await client.PostAsJsonAsync("/api/auth/registro", new
        {
            nombre = "Cliente",
            apellido = "E2E",
            email,
            password
        });
        Assert.Equal(HttpStatusCode.OK, registro.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var sesion = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(sesion);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", sesion.Token);

        var productos = await client.GetFromJsonAsync<List<ProductoDto>>("/api/productos");
        var producto = Assert.Single(productos!);

        var agregar = await client.PostAsJsonAsync("/api/carrito/items", new
        {
            productoId = producto.Id,
            varianteId = (int?)null,
            cantidad = 2
        });
        agregar.EnsureSuccessStatusCode();

        var checkout = await client.PostAsJsonAsync("/api/ordenes/checkout", new
        {
            numeroTarjeta = "4111111111111111",
            nombreTitular = "Cliente E2E",
            vencimiento = "12/30",
            cvv = "123"
        });
        checkout.EnsureSuccessStatusCode();
        var orden = await checkout.Content.ReadFromJsonAsync<OrdenDto>();
        Assert.NotNull(orden);
        Assert.Equal(2, orden.Items.Single().Cantidad);

        var historial = await client.GetFromJsonAsync<List<OrdenDto>>("/api/ordenes/mis-compras");
        var compra = Assert.Single(historial!);
        Assert.Equal(orden.Id, compra.Id);

        var carrito = await client.GetFromJsonAsync<CarritoDto>("/api/carrito");
        Assert.NotNull(carrito);
        Assert.Empty(carrito.Items);
    }

    [Fact]
    public async Task Health_checks_informan_estado_live_y_ready()
    {
        using var client = _factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        live.EnsureSuccessStatusCode();
        ready.EnsureSuccessStatusCode();
        Assert.Contains("Healthy", await live.Content.ReadAsStringAsync());
        Assert.Contains("database", await ready.Content.ReadAsStringAsync());
    }
}

public sealed class EcommerceApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly Dictionary<string, string?> _variablesAnteriores = new();

    public EcommerceApiFactory()
    {
        ConfigurarVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        ConfigurarVariable(
            "ConnectionStrings__DefaultConnection",
            "Server=(localdb)\\mssqllocaldb;Database=IgnoredByTests");
        ConfigurarVariable("Jwt__Key", "clave-de-pruebas-con-mas-de-32-caracteres-segura");
        ConfigurarVariable("Jwt__Issuer", "EcommerceApiTests");
        ConfigurarVariable("Jwt__Audience", "EcommerceAppTests");
        ConfigurarVariable("Jwt__ExpiracionMinutos", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=IgnoredByTests",
                ["Jwt:Key"] = "clave-de-pruebas-con-mas-de-32-caracteres-segura",
                ["Jwt:Issuer"] = "EcommerceApiTests",
                ["Jwt:Audience"] = "EcommerceAppTests",
                ["Jwt:ExpiracionMinutos"] = "60"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            services.AddHostedService<TestDatabaseInitializer>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _connection.Dispose();
        foreach (var variable in _variablesAnteriores)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
    }

    private void ConfigurarVariable(string nombre, string valor)
    {
        _variablesAnteriores[nombre] = Environment.GetEnvironmentVariable(nombre);
        Environment.SetEnvironmentVariable(nombre, valor);
    }
}

internal sealed class TestDatabaseInitializer(
    IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.CreateAsync(new IdentityRole("Cliente"));

        context.Productos.Add(new Producto
        {
            Nombre = "Producto E2E",
            Descripcion = "Producto para la prueba completa",
            Precio = 1500m,
            Stock = 10,
            Categoria = new Categoria { Nombre = "Pruebas E2E" }
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
