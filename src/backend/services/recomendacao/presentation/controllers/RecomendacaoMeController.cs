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
[Authorize(Policy = AuthSetup.PolicyCadastrado)]
public class RecomendacaoMeController : ControllerBase
{
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
