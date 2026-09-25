using Microsoft.AspNetCore.Mvc;
using Ecommerce.Api.DTOs;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.RateLimiting;


namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("autenticacion")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/registro
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponseDto>> Registro(RegistroDto dto)
    {
        var resultado = await _authService.RegistrarAsync(dto);

        if (!resultado.Exito)
        {
            return BadRequest(new { mensaje = resultado.MensajeError });
        }

        return Ok(resultado.Datos);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (!resultado.Exito)
        {
            return Unauthorized(new { mensaje = resultado.MensajeError });
        }

        return Ok(resultado.Datos);
    }
}
