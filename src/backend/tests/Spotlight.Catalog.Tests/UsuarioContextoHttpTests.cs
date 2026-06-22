using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Spotlight.Catalogo.Infrastructure.Auth;
using Xunit;

namespace Spotlight.Catalogo.Tests;

public class UsuarioContextoHttpTests
{
    [Fact]
    public void UsuarioId_ComNameIdentifierValido_RetornaGuid()
    {
        var usuarioId = Guid.NewGuid();
        var contexto = CriarContexto(new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()));

        Assert.Equal(usuarioId, contexto.UsuarioId);
    }

    [Fact]
    public void UsuarioId_ComClaimSubValida_RetornaGuid()
    {
        var usuarioId = Guid.NewGuid();
        var contexto = CriarContexto(new Claim("sub", usuarioId.ToString()));

        Assert.Equal(usuarioId, contexto.UsuarioId);
    }

    [Fact]
    public void UsuarioId_ComClaimInvalida_RetornaNull()
    {
        var contexto = CriarContexto(new Claim("sub", "nao-e-guid"));

        Assert.Null(contexto.UsuarioId);
    }

    [Fact]
    public void UsuarioId_SemHttpContext_RetornaNull()
    {
        var contexto = new UsuarioContextoHttp(new HttpContextAccessor());

        Assert.Null(contexto.UsuarioId);
    }

    private static UsuarioContextoHttp CriarContexto(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };

        return new UsuarioContextoHttp(new HttpContextAccessor { HttpContext = httpContext });
    }
}
