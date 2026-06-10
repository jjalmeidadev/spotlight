using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Spotlight.Catalogo.Tests.Integration;

/// <summary>Gera JWTs HS256 compatíveis com <see cref="Spotlight.Catalogo.Infrastructure.Auth.AuthSetup"/>.</summary>
internal static class JwtTestHelper
{
    public static string CriarToken(string tipoUsuario, string? sub = null)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(CatalogoIntegrationFixture.JwtSecret));
        var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("tipo_usuario", tipoUsuario),
            new Claim("sub", sub ?? Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
