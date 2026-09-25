using System.Reflection;
using Ecommerce.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Xunit;

namespace Ecommerce.Api.Tests;

public class AutorizacionTests
{
    [Theory]
    [InlineData(typeof(CarritoController))]
    [InlineData(typeof(OrdenesController))]
    public void Recursos_del_usuario_requieren_autenticacion(Type controller)
    {
        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Theory]
    [InlineData(nameof(ProductosController.CrearProducto))]
    [InlineData(nameof(ProductosController.ActualizarProducto))]
    [InlineData(nameof(ProductosController.EliminarProducto))]
    [InlineData(nameof(ProductosController.SubirImagen))]
    public void Mutaciones_de_productos_requieren_rol_admin(string metodo)
    {
        var action = typeof(ProductosController).GetMethod(metodo);
        var authorize = action?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize.Roles);
    }

    [Fact]
    public void Autenticacion_tiene_rate_limiting()
    {
        var rateLimit = typeof(AuthController).GetCustomAttribute<EnableRateLimitingAttribute>();

        Assert.NotNull(rateLimit);
        Assert.Equal("autenticacion", rateLimit.PolicyName);
    }
}
