using Microsoft.AspNetCore.Identity;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, IWebHostEnvironment entorno)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Ecommerce.Api.Data.DbInitializer");

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration, logger);
        await SeedCuponAsync(context);


        if (entorno.IsDevelopment())
        {
            await SeedCatalogoDePruebaAsync(context);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Cliente"];

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation(
                "AdminSeed no está configurado; se omite la creación del administrador inicial");
            return;
        }

        var adminExistente = await userManager.FindByEmailAsync(email);
        if (adminExistente is not null) return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nombre = "Admin",
            Apellido = "Sistema",
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(admin, password);
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            var errores = string.Join(" | ", resultado.Errors.Select(e => e.Description));
            logger.LogError(
                "No se pudo crear el administrador inicial. Errores: {IdentityErrors}",
                errores);
        }
    }

    private static async Task SeedCuponAsync(AppDbContext context)
    {
        if (context.Cupones.Any()) return;

        context.Cupones.Add(new CuponDescuento
        {
            Codigo = "BIENVENIDO10",
            PorcentajeDescuento = 10,
            Activo = true,
            UsoMaximo = null
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedCatalogoDePruebaAsync(AppDbContext context)
    {
        if (context.Categorias.Any()) return;

        var categorias = new List<Categoria>
        {
            new() { Nombre = "Calzado", MostrarEnHome = true, Orden = 0 },
            new() { Nombre = "Indumentaria", MostrarEnHome = true, Orden = 1 },
            new() { Nombre = "Accesorios", MostrarEnHome = true, Orden = 2 }
        };

        context.Categorias.AddRange(categorias);
        await context.SaveChangesAsync();

        context.Productos.AddRange(
            new Producto
            {
                Nombre = "Zapatillas Running Pro",
                Descripcion = "Zapatillas livianas para correr largas distancias",
                Precio = 89999,
                Stock = 15,
                Destacado = true,
                CategoriaId = categorias[0].Id
            },
            new Producto
            {
                Nombre = "Remera Deportiva Dry-Fit",
                Descripcion = "Tela que absorbe la humedad",
                Precio = 24999,
                Stock = 30,
                Destacado = true,
                CategoriaId = categorias[1].Id
            },
            new Producto
            {
                Nombre = "Mochila Urbana 20L",
                Descripcion = "Resistente al agua, compartimento para notebook",
                Precio = 45999,
                Stock = 8,
                Destacado = false,
                CategoriaId = categorias[2].Id
            }
        );

        await context.SaveChangesAsync();
    }
}
