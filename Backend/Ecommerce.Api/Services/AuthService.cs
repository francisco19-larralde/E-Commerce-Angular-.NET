using Microsoft.AspNetCore.Identity;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Services;

public class AuthService : IAuthService
{
    private const string MensajeLoginInvalido =
        "Email o contraseña incorrectos, o cuenta temporalmente bloqueada";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<ResultadoOperacion<AuthResponseDto>> RegistrarAsync(RegistroDto dto)
    {
        var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
        if (usuarioExistente is not null)
        {
            return ResultadoOperacion<AuthResponseDto>.Fallo(
                "Ya existe una cuenta registrada con ese email", TipoError.ValidacionNegocio);
        }

        var nuevoUsuario = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido
        };


        var resultado = await _userManager.CreateAsync(nuevoUsuario, dto.Password);

        if (!resultado.Succeeded)
        {
            var errores = string.Join(" | ", resultado.Errors.Select(e => e.Description));
            return ResultadoOperacion<AuthResponseDto>.Fallo(errores, TipoError.ValidacionNegocio);
        }


        await _userManager.AddToRoleAsync(nuevoUsuario, "Cliente");

        var respuesta = GenerarRespuesta(nuevoUsuario, ["Cliente"]);
        return ResultadoOperacion<AuthResponseDto>.Ok(respuesta);
    }

    public async Task<ResultadoOperacion<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);
        if (usuario is null)
        {
            return ResultadoOperacion<AuthResponseDto>.Fallo(
                MensajeLoginInvalido, TipoError.ValidacionNegocio);
        }

        var inicioSesion = await _signInManager.CheckPasswordSignInAsync(
            usuario,
            dto.Password,
            lockoutOnFailure: true);

        if (inicioSesion.IsLockedOut)
        {
            return ResultadoOperacion<AuthResponseDto>.Fallo(
                MensajeLoginInvalido, TipoError.ValidacionNegocio);
        }

        if (!inicioSesion.Succeeded)
        {
            return ResultadoOperacion<AuthResponseDto>.Fallo(
                MensajeLoginInvalido, TipoError.ValidacionNegocio);
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var respuesta = GenerarRespuesta(usuario, roles);
        return ResultadoOperacion<AuthResponseDto>.Ok(respuesta);
    }

    private AuthResponseDto GenerarRespuesta(ApplicationUser usuario, IList<string> roles)
    {
        var (token, expiracion) = _tokenService.GenerarToken(usuario, roles);

        return new AuthResponseDto
        {
            Token = token,
            Expiracion = expiracion,
            Nombre = $"{usuario.Nombre} {usuario.Apellido}",
            Email = usuario.Email!,
            Roles = roles.ToList()
        };
    }
}
