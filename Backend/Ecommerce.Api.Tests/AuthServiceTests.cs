using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecommerce.Api.Tests;

public sealed class AuthServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private ServiceProvider _provider = null!;
    private AsyncServiceScope _scope;
    private UserManager<ApplicationUser> _userManager = null!;
    private AuthService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<ITokenService, TokenServiceFalso>();
        services.AddScoped<AuthService>();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();

        var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _service = _scope.ServiceProvider.GetRequiredService<AuthService>();

        var usuario = new ApplicationUser
        {
            UserName = "cliente@example.test",
            Email = "cliente@example.test",
            Nombre = "Cliente",
            Apellido = "Prueba"
        };
        var creado = await _userManager.CreateAsync(usuario, "Segura1!");
        Assert.True(creado.Succeeded);
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Cinco_intentos_fallidos_bloquean_temporalmente_la_cuenta()
    {
        for (var intento = 0; intento < 5; intento++)
        {
            var resultado = await _service.LoginAsync(new LoginDto
            {
                Email = "cliente@example.test",
                Password = "Incorrecta1!"
            });

            Assert.False(resultado.Exito);
        }

        var usuario = await _userManager.FindByEmailAsync("cliente@example.test");
        Assert.NotNull(usuario);
        Assert.True(await _userManager.IsLockedOutAsync(usuario));
        Assert.True(usuario.LockoutEnd > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_exitoso_reinicia_el_contador_de_fallos()
    {
        await _service.LoginAsync(new LoginDto
        {
            Email = "cliente@example.test",
            Password = "Incorrecta1!"
        });
        await _service.LoginAsync(new LoginDto
        {
            Email = "cliente@example.test",
            Password = "Incorrecta1!"
        });

        var resultado = await _service.LoginAsync(new LoginDto
        {
            Email = "cliente@example.test",
            Password = "Segura1!"
        });

        var usuario = await _userManager.FindByEmailAsync("cliente@example.test");
        Assert.True(resultado.Exito);
        Assert.NotNull(usuario);
        Assert.Equal(0, await _userManager.GetAccessFailedCountAsync(usuario));
    }

    private sealed class TokenServiceFalso : ITokenService
    {
        public (string Token, DateTime Expiracion) GenerarToken(
            ApplicationUser usuario,
            IList<string> roles) =>
            ("token-prueba", DateTime.UtcNow.AddHours(1));
    }
}
