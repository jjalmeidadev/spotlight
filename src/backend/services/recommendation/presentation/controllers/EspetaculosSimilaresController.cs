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
[Produces("application/json")]
public class EspetaculosSimilaresController : ControllerBase
{
    /// <summary>
    /// Lista espetáculos similares a um espetáculo de origem (similaridade item-item).
    /// </summary>
    /// <remarks>
    /// Contrato público: <c>GET /recomendacao/espetaculos/{id}/similares?limite={n}</c>.
    /// Rota pública — não requer JWT.
    ///
    /// O serviço lê de <c>similaridade_item</c> (similaridade global pré-calculada)
    /// e retorna os <i>n</i> vizinhos mais próximos do espetáculo de origem, com
    /// score e nome do algoritmo usado.
    ///
    /// Exemplo de requisição:
    /// <code>
    /// curl "https://api.spotlight.app/recomendacao/espetaculos/42/similares?limite=5"
    /// </code>
    ///
    /// Exemplo de resposta (200):
    /// <code>
    /// {
    ///   "data": [
    ///     {
    ///       "espetaculo": {
    ///         "id": 87,
    ///         "titulo": "Macbeth",
    ///         "genero": "Drama",
    ///         "status": "ativo",
    ///         "notaMedia": 4.5,
    ///         "midias": [
    ///           {
    ///             "tipoMidia": "capa",
    ///             "storageBucket": "espetaculos",
    ///             "storagePath": "87/capa.jpg",
    ///             "altText": "Cartaz de Macbeth",
    ///             "ordemExibicao": 1
    ///           }
    ///         ]
    ///       },
    ///       "score": 0.81,
    ///       "tipoAlgoritmo": "cosine"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    /// <param name="id">Identificador do espetáculo de origem (inteiro &gt; 0).</param>
    /// <param name="limite">
    /// Quantidade máxima de similares retornados. Aceita valores entre <b>1</b>
    /// e <b>50</b>; valores fora desse intervalo (ou ausentes) são normalizados
    /// para o padrão <b>10</b>.
    /// </param>
    /// <param name="listar">Caso de uso resolvido por DI (não enviado pelo cliente).</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de similares no envelope <c>{ "data": [...] }</c>. Pode vir vazia se ainda não houver similaridade calculada para o item.</response>
    /// <response code="404">Espetáculo de origem não encontrado.</response>
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
