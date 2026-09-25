using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;

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

    public async Task<string> GuardarAsync(
        IFormFile archivo,
        string extension,
        string nombreArchivo,
        string urlBase)
    {
        var carpeta = ObtenerCarpeta();
        Directory.CreateDirectory(carpeta);

        var nombreSeguro = NormalizarNombreArchivo(nombreArchivo);
        var nombreFinal = $"{nombreSeguro}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreFinal);
        var rutaTemporal = Path.Combine(carpeta, $".{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(rutaTemporal, FileMode.CreateNew, FileAccess.Write))
            {
                await archivo.CopyToAsync(stream);
            }

            File.Move(rutaTemporal, rutaFisica, overwrite: true);
        }
        finally
        {
            if (File.Exists(rutaTemporal)) File.Delete(rutaTemporal);
        }

        var basePublica = _urlPublicaConfigurada ?? urlBase.TrimEnd('/');
        return $"{basePublica}/uploads/productos/{nombreFinal}";
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

    private static string NormalizarNombreArchivo(string nombre)
    {
        var normalizado = nombre.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder();
        var separadorPendiente = false;

        foreach (var caracter in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(caracter))
            {
                if (separadorPendiente && resultado.Length > 0) resultado.Append('-');
                resultado.Append(char.ToLowerInvariant(caracter));
                separadorPendiente = false;
            }
            else
            {
                separadorPendiente = true;
            }

            if (resultado.Length >= 120) break;
        }

        return resultado.Length == 0 ? "producto" : resultado.ToString().TrimEnd('-');
    }
}
