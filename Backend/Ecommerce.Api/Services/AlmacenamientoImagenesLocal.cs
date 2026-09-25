using Microsoft.AspNetCore.Http;

namespace Ecommerce.Api.Services;

public class AlmacenamientoImagenesLocal : IAlmacenamientoImagenes
{
    private readonly IWebHostEnvironment _entorno;
    private readonly string? _urlPublicaConfigurada;

    public AlmacenamientoImagenesLocal(IWebHostEnvironment entorno, IConfiguration configuration)
    {
        _entorno = entorno;
        _urlPublicaConfigurada = configuration["ImageStorage:PublicBaseUrl"]?.TrimEnd('/');
    }

    public async Task<string> GuardarAsync(IFormFile archivo, string extension, string urlBase)
    {
        var carpeta = ObtenerCarpeta();
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using var stream = new FileStream(rutaFisica, FileMode.CreateNew, FileAccess.Write);
        await archivo.CopyToAsync(stream);

        var basePublica = _urlPublicaConfigurada ?? urlBase.TrimEnd('/');
        return $"{basePublica}/uploads/productos/{nombreArchivo}";
    }

    public Task EliminarAsync(string? urlImagen)
    {
        if (string.IsNullOrWhiteSpace(urlImagen)) return Task.CompletedTask;

        var rutaUrl = Uri.TryCreate(urlImagen, UriKind.Absolute, out var uri)
            ? uri.LocalPath
            : urlImagen;
        var nombreArchivo = Path.GetFileName(rutaUrl);
        var rutaFisica = Path.Combine(ObtenerCarpeta(), nombreArchivo);

        if (File.Exists(rutaFisica)) File.Delete(rutaFisica);
        return Task.CompletedTask;
    }

    private string ObtenerCarpeta()
    {
        var raiz = _entorno.WebRootPath ?? Path.Combine(_entorno.ContentRootPath, "wwwroot");
        return Path.Combine(raiz, "uploads", "productos");
    }
}
