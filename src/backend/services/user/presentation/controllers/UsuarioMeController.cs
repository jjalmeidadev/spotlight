using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Presentation.Controllers;

/// <summary>
/// Rotas do proprio usuario autenticado. O gateway remove o prefixo /usuario,
/// entao a rota interna do servico e /me.
/// </summary>
[ApiController]
[Route("me")]
[Authorize]
public class UsuarioMeController : ControllerBase
{
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

    [HttpGet("preferencias")]
    [ProducesResponseType(typeof(PreferenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPreferencias(
        [FromServices] ObterPreferenciasUseCase obter,
        [FromServices] IUsuarioRepository repositorio,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        var usuarioId = ExigirUsuario(usuarioContexto);
        await GarantirPerfilAsync(repositorio, usuarioContexto, usuarioId, cancellationToken);
        return Ok(await obter.ExecutarAsync(usuarioId, cancellationToken));
    }

    [HttpPut("preferencias")]
    [ProducesResponseType(typeof(PreferenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarPreferencias(
        [FromBody] PreferenciasDto body,
        [FromServices] AtualizarPreferenciasUseCase atualizar,
        [FromServices] IUsuarioRepository repositorio,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        var usuarioId = ExigirUsuario(usuarioContexto);
        await GarantirPerfilAsync(repositorio, usuarioContexto, usuarioId, cancellationToken);
        return Ok(await atualizar.ExecutarAsync(usuarioId, body, cancellationToken));
    }

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

    private static Guid ExigirUsuario(IUsuarioContexto ctx)
        => ctx.UsuarioId ?? throw new ValidationException(new Dictionary<string, string[]>
            { ["sub"] = ["Claim 'sub' ausente ou invalida no token."] });

    private static Task GarantirPerfilAsync(
        IUsuarioRepository repositorio,
        IUsuarioContexto ctx,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ctx.Email))
        {
            return Task.CompletedTask;
        }

        return repositorio.GarantirPerfilAsync(
            usuarioId,
            ctx.Email,
            ctx.Nome,
            ctx.DataNascimento,
            cancellationToken);
    }
}
