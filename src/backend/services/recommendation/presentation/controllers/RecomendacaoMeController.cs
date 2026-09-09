using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;
using Spotlight.Recomendacao.Infrastructure.Auth;

namespace Spotlight.Recomendacao.Presentation.Controllers;

/// <summary>
/// Recomendações personalizadas do usuário autenticado. O prefixo
/// <c>/recomendacao</c> é removido pelo API Gateway, então a rota interna do
/// serviço é <c>me</c> (contrato público: <c>GET /recomendacao/me</c>).
/// </summary>
[ApiController]
[Route("me")]
[Produces("application/json")]
[Authorize(Policy = AuthSetup.PolicyCadastrado)]
public class RecomendacaoMeController : ControllerBase
{
    /// <summary>
    /// Lista recomendações personalizadas para o usuário autenticado.
    /// </summary>
    /// <remarks>
    /// Contrato público: <c>GET /recomendacao/me?limite={n}</c> (o prefixo
    /// <c>/recomendacao</c> é removido pelo API Gateway antes do encaminhamento).
    ///
    /// A identidade do usuário é extraída da claim <c>sub</c> do JWT do Supabase
    /// — não é necessário enviar o id na rota. A regra escolhe automaticamente
    /// entre o modelo ML.NET (quando os artefatos estão disponíveis) e o
    /// fallback pré-processado no PostgreSQL.
    ///
    /// Exemplo de requisição:
    /// <code>
    /// curl -H "Authorization: Bearer {jwt}" \
    ///      "https://api.spotlight.app/recomendacao/me?limite=5"
    /// </code>
    ///
    /// Exemplo de resposta (200):
    /// <code>
    /// {
    ///   "data": [
    ///     {
    ///       "espetaculo": {
    ///         "id": 42,
    ///         "titulo": "Hamlet",
    ///         "genero": "Drama",
    ///         "status": "ativo",
    ///         "notaMedia": 4.7,
    ///         "midias": [
    ///           {
    ///             "tipoMidia": "capa",
    ///             "storageBucket": "espetaculos",
    ///             "storagePath": "42/capa.jpg",
    ///             "altText": "Cartaz de Hamlet",
    ///             "ordemExibicao": 1
    ///           }
    ///         ]
    ///       },
    ///       "score": 0.92,
    ///       "tipoAlgoritmo": "ml_net",
    ///       "motivo": "preferencias_genero",
    ///       "dataGeracao": "2026-06-12T14:30:00Z"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    /// <param name="limite">
    /// Quantidade máxima de itens retornados. Aceita valores entre <b>1</b> e
    /// <b>50</b>; valores fora desse intervalo (ou ausentes) são normalizados
    /// para o padrão <b>10</b>.
    /// </param>
    /// <param name="listar">Caso de uso resolvido por DI (não enviado pelo cliente).</param>
    /// <param name="usuarioContexto">Contexto do usuário autenticado (não enviado pelo cliente).</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de recomendações no envelope <c>{ "data": [...] }</c>. Pode vir vazia se não houver sugestões.</response>
    /// <response code="401">JWT ausente, expirado ou inválido.</response>
    /// <response code="403">Token válido, mas o usuário não tem papel <c>cadastrado</c>.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListaDto<RecomendacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Obter(
        [FromQuery] int? limite,
        [FromServices] ListarRecomendacoesMeUseCase listar,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        var usuarioId = usuarioContexto.UsuarioId
            ?? throw new ValidationException(new Dictionary<string, string[]>
                { ["sub"] = ["Claim 'sub' ausente ou inválida no token."] });

        return Ok(await listar.ExecutarAsync(usuarioId, limite, cancellationToken));
    }
}
