using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Recomendacao.Presentation.Controllers;

[ApiController]
[Route("eventos/avaliacoes")]
public class AvaliacaoEventosController(ILogger<AvaliacaoEventosController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult Receber([FromBody] AvaliacaoCriadaEventoDto evento)
    {
        logger.LogInformation(
            "Avaliacao {AvaliacaoId} recebida para recalculo de recomendacao. Usuario={UsuarioId}; Espetaculo={EspetaculoId}; Nota={Nota}",
            evento.AvaliacaoId,
            evento.UsuarioId,
            evento.EspetaculoId,
            evento.Nota);

        return Accepted(new { status = "accepted" });
    }
}

public sealed class AvaliacaoCriadaEventoDto
{
    public int AvaliacaoId { get; init; }
    public int EspetaculoId { get; init; }
    public Guid UsuarioId { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public DateTime CreatedAt { get; init; }
}
