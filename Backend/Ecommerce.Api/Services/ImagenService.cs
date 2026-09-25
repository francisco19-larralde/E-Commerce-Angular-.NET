using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Ecommerce.Api.Data;

namespace Ecommerce.Api.Services;

public class ImagenService : IImagenService
{
    private readonly AppDbContext _context;
    private readonly IAlmacenamientoImagenes _almacenamiento;

    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TamanioMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public ImagenService(AppDbContext context, IAlmacenamientoImagenes almacenamiento)
    {
        _context = context;
        _almacenamiento = almacenamiento;
    }

    public async Task<ResultadoOperacion<string>> SubirImagenAsync(int productoId, IFormFile archivo, string urlBase)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        if (producto is null)
        {
            return ResultadoOperacion<string>.Fallo("El producto no existe", TipoError.NoEncontrado);
        }

        if (archivo.Length == 0)
        {
            return ResultadoOperacion<string>.Fallo("El archivo está vacío", TipoError.ValidacionNegocio);
        }

        if (archivo.Length > TamanioMaximoBytes)
        {
            return ResultadoOperacion<string>.Fallo("La imagen no puede pesar más de 5 MB", TipoError.ValidacionNegocio);
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            return ResultadoOperacion<string>.Fallo(
                "Formato no permitido. Usá JPG, PNG o WEBP", TipoError.ValidacionNegocio);
        }

        var urlAnterior = producto.ImagenUrl;
        var nombreArchivo = $"{producto.Nombre}-{producto.Id}";
        var urlPublica = await _almacenamiento.GuardarAsync(
            archivo,
            extension,
            nombreArchivo,
            urlBase);
        producto.ImagenUrl = urlPublica;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            if (!string.Equals(urlAnterior, urlPublica, StringComparison.OrdinalIgnoreCase))
            {
                await _almacenamiento.EliminarAsync(urlPublica);
            }
            throw;
        }

        if (!string.Equals(urlAnterior, urlPublica, StringComparison.OrdinalIgnoreCase))
        {
            await _almacenamiento.EliminarAsync(urlAnterior);
        }

        return ResultadoOperacion<string>.Ok(urlPublica);
    }

    public async Task<ResultadoOperacion> EliminarImagenAsync(int productoId)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        if (producto is null)
        {
            return ResultadoOperacion.Fallo("El producto no existe", TipoError.NoEncontrado);
        }

        var urlAnterior = producto.ImagenUrl;
        producto.ImagenUrl = null;
        await _context.SaveChangesAsync();
        await _almacenamiento.EliminarAsync(urlAnterior);

        return ResultadoOperacion.Ok();
    }
}
