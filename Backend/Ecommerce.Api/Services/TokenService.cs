using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ecommerce.Api.Models;
using Ecommerce.Api.Configuration;

namespace Ecommerce.Api.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(JwtOptions options)
    {
        _options = options;
    }

    public (string Token, DateTime Expiracion) GenerarToken(ApplicationUser usuario, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}")
        };


        claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiracion = DateTime.UtcNow.AddMinutes(_options.ExpiracionMinutos);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiracion);
    }
}
