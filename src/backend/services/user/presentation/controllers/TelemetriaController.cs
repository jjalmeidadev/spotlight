using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;

namespace Spotlight.Usuario.Presentation.Controllers;

/// <summary>
/// Recebe eventos comportamentais do aplicativo para persistencia no MongoDB.
/// Rota interna: <c>/telemetria</c>. Contrato publico via gateway:
/// <c>POST /usuario/telemetria</c>.
/// </summary>
[ApiController]
[Route("telemetria")]
public sealed class TelemetriaController : ControllerBase
{
    /// <summary>
    /// Registra um evento comportamental (impressão, clique, conversão) para persistência no MongoDB.
    /// Retorna <c>202 Accepted</c> com o identificador do evento. Aceita requisições com ou sem JWT —
    /// <c>user_id</c> é gravado quando o token é válido; caso contrário usa o <c>anonymous_id</c> do cliente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarTelemetriaResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarTelemetriaRequest request,
        CancellationToken cancellationToken)
    {
        var service = HttpContext.RequestServices.GetService<ITelemetriaService>();
        if (service is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErroDto
            {
                Error = "Telemetria nao configurada.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var usuarioId = ObterUsuarioIdOpcional();
        var eventId = await service.RegistrarAsync(usuarioId, request, cancellationToken);

        return StatusCode(
            StatusCodes.Status202Accepted,
            new RegistrarTelemetriaResponse(eventId, "accepted"));
    }

    private Guid? ObterUsuarioIdOpcional()
    {
        var authorization = Request.Headers.Authorization.ToString();
        var tokenFoiEnviado = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

        if (tokenFoiEnviado && User.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Token ausente ou invalido.");
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(sub))
        {
            return null;
        }

        return Guid.TryParse(sub, out var usuarioId)
            ? usuarioId
            : throw new UnauthorizedAccessException("Token sem identificador de usuario valido.");
    }
}
