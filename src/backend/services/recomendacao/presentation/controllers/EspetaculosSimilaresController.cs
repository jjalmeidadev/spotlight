using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Presentation.Controllers;

/// <summary>
/// Similares globais (item-item). O prefixo <c>/recomendacao</c> é removido
/// pelo API Gateway, então a rota interna do serviço é <c>espetaculos/{id}/similares</c>
/// (contrato público: <c>GET /recomendacao/espetaculos/{id}/similares</c>).
/// Rota pública — não requer autenticação.
/// </summary>
[ApiController]
[Route("espetaculos")]
public class EspetaculosSimilaresController : ControllerBase
{
    [HttpGet("{id:int}/similares")]
    [ProducesResponseType(typeof(ListaDto<SimilarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(
        int id,
        [FromQuery] int? limite,
        [FromServices] ListarSimilaresUseCase listar,
        CancellationToken cancellationToken)
    {
        return Ok(await listar.ExecutarAsync(id, limite, cancellationToken));
    }
}
