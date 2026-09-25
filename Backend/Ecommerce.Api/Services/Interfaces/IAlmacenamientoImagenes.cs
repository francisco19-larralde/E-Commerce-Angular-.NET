using Microsoft.AspNetCore.Http;

namespace Ecommerce.Api.Services;

public interface IAlmacenamientoImagenes
{
    Task<string> GuardarAsync(IFormFile archivo, string extension, string urlBase);
    Task EliminarAsync(string? urlImagen);
}
