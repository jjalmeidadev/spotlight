using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;
using Spotlight.Usuario.Infrastructure.Auth;

namespace Spotlight.Usuario.Presentation.Controllers;

/// <summary>
/// Rotas do próprio usuário autenticado. O prefixo <c>/usuario</c> é removido
/// pelo API Gateway, então a rota interna do serviço é <c>me</c>
/// (contrato público: <c>GET /usuario/me</c>).
/// Todas as rotas exigem <c>cadastrado</c>.
/// </summary>
[ApiController]
[Route("me")]
[Authorize(Policy = AuthSetup.PolicyCadastrado)]
public class UsuarioMeController : ControllerBase
{
    /// <summary>Perfil do usuário autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Obter(
        [FromServices] ObterUsuarioMeUseCase obter,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await obter.ExecutarAsync(ExigirUsuario(usuarioContexto), cancellationToken));
    }

    /// <summary>Atualização parcial do próprio perfil.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Atualizar(
        [FromBody] UsuarioUpdateDto body,
        [FromServices] AtualizarUsuarioMeUseCase atualizar,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await atualizar.ExecutarAsync(ExigirUsuario(usuarioContexto), body, cancellationToken));
    }

    /// <summary>Preferências do usuário (campo <c>perfil_preferencias</c> JSONB).</summary>
    [HttpGet("preferencias")]
    [ProducesResponseType(typeof(PreferenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPreferencias(
        [FromServices] ObterPreferenciasUseCase obter,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await obter.ExecutarAsync(ExigirUsuario(usuarioContexto), cancellationToken));
    }

    /// <summary>Substituição das preferências (semântica PUT — replace, não merge).</summary>
    [HttpPut("preferencias")]
    [ProducesResponseType(typeof(PreferenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarPreferencias(
        [FromBody] PreferenciasDto body,
        [FromServices] AtualizarPreferenciasUseCase atualizar,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await atualizar.ExecutarAsync(ExigirUsuario(usuarioContexto), body, cancellationToken));
    }

    /// <summary>Histórico de avaliações do usuário (paginado).</summary>
    [HttpGet("avaliacoes")]
    [ProducesResponseType(typeof(ListaPaginadaDto<AvaliacaoUsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAvaliacoes(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ListarAvaliacoesUsuarioUseCase listar,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await listar.ExecutarAsync(
            ExigirUsuario(usuarioContexto), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Extrai o usuário do contexto ou lança erro estruturado se o JWT veio sem <c>sub</c>
    /// (estado anômalo — token válido mas mal formado).
    /// </summary>
    private static Guid ExigirUsuario(IUsuarioContexto ctx)
        => ctx.UsuarioId ?? throw new ValidationException(new Dictionary<string, string[]>
            { ["sub"] = ["Claim 'sub' ausente ou inválida no token."] });
}
