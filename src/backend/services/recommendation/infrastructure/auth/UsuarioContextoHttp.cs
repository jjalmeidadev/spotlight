using System.Security.Claims;
using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Infrastructure.Auth;

/// <summary>
/// Implementação de <see cref="IUsuarioContexto"/> que lê o claim <c>sub</c> do
/// JWT validado (Supabase Auth). Registrar como <c>Scoped</c>.
/// </summary>
public class UsuarioContextoHttp(IHttpContextAccessor accessor) : IUsuarioContexto
{
    public Guid? UsuarioId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
