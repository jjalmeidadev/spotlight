using System.Security.Claims;
using System.Text.Json;
using Spotlight.Usuario.Application.UseCases;

namespace Spotlight.Usuario.Infrastructure.Auth;

/// <summary>
/// Implementacao de <see cref="IUsuarioContexto"/> que le claims do JWT validado.
/// </summary>
public class UsuarioContextoHttp(IHttpContextAccessor accessor) : IUsuarioContexto
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid? UsuarioId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email =>
        FirstNotBlank(
            User?.FindFirstValue(ClaimTypes.Email),
            User?.FindFirstValue("email"));

    public string? Nome =>
        FirstNotBlank(
            GetUserMetadataValue("nome"),
            User?.FindFirstValue(ClaimTypes.Name),
            User?.FindFirstValue("name"));

    public DateOnly? DataNascimento
    {
        get
        {
            var value = GetUserMetadataValue("dataNascimento")
                        ?? User?.FindFirstValue("dataNascimento");
            return DateOnly.TryParse(value, out var date) ? date : null;
        }
    }

    private string? GetUserMetadataValue(string propertyName)
    {
        var raw = User?.FindFirstValue("user_metadata");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            return document.RootElement.TryGetProperty(propertyName, out var property)
                ? property.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FirstNotBlank(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
