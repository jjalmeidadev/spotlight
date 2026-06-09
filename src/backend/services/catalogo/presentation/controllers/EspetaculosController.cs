using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Spotlight.Catalogo.Infrastructure.Auth;

namespace Spotlight.Catalogo.Presentation.Controllers;

/// <summary>
/// Rotas de espetáculos do catálogo. O prefixo <c>/catalogo</c> é removido pelo
/// API Gateway, então a rota interna do serviço é <c>espetaculos</c>
/// (contrato público: <c>GET /catalogo/espetaculos</c>).
/// </summary>
[ApiController]
[Route("espetaculos")]
[Route("catalogo/espetaculos")]
public class EspetaculosController : ControllerBase
{
    /// <summary>Lista espetáculos do catálogo, com filtros e paginação. Rota pública.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListaPaginadaDto<EspetaculoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromServices] ListarEspetaculosUseCase listarEspetaculos,
        [FromQuery] ListarEspetaculosFiltro filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await listarEspetaculos.ExecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Detalhe de um espetáculo (com categorias, atores e sessões). Rota pública.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EspetaculoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] int id,
        [FromServices] ObterEspetaculoUseCase obter,
        CancellationToken cancellationToken)
    {
        var resultado = await obter.ExecutarAsync(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Lista sessões de um espetáculo. Rota pública.</summary>
    [HttpGet("{id:int}/sessoes")]
    [ProducesResponseType(typeof(ListaDto<SessaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarSessoes(
        [FromRoute] int id,
        [FromServices] ListarSessoesUseCase listar,
        CancellationToken cancellationToken)
    {
        var resultado = await listar.ExecutarAsync(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Lista avaliações de um espetáculo (paginadas). Rota pública.</summary>
    [HttpGet("{id:int}/avaliacoes")]
    [ProducesResponseType(typeof(ListaPaginadaDto<AvaliacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAvaliacoes(
        [FromRoute] int id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ListarAvaliacoesUseCase listar,
        CancellationToken cancellationToken)
    {
        var resultado = await listar.ExecutarAsync(id, page, pageSize, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Cria uma avaliação para o espetáculo. Requer <c>cadastrado</c>.</summary>
    [HttpPost("{id:int}/avaliacoes")]
    [Authorize(Policy = AuthSetup.PolicyCadastrado)]
    [ProducesResponseType(typeof(AvaliacaoCriadaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarAvaliacao(
        [FromRoute] int id,
        [FromBody] AvaliacaoCreateDto body,
        [FromServices] CriarAvaliacaoUseCase criar,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        var usuarioId = usuarioContexto.UsuarioId
            ?? throw new ValidationException(new Dictionary<string, string[]>
                { ["sub"] = ["Claim 'sub' ausente ou inválida no token."] });

        var resultado = await criar.ExecutarAsync(id, usuarioId, body, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    /// <summary>Cria um espetáculo. Requer <c>administrador</c>.</summary>
    [HttpPost]
    [Authorize(Policy = AuthSetup.PolicyAdministrador)]
    [ProducesResponseType(typeof(EspetaculoDetalheDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar(
        [FromBody] EspetaculoCreateDto body,
        [FromServices] CriarEspetaculoUseCase criar,
        CancellationToken cancellationToken)
    {
        var detalhe = await criar.ExecutarAsync(body, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = detalhe.Id }, detalhe);
    }

    /// <summary>Atualiza parcialmente um espetáculo. Requer <c>administrador</c>.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthSetup.PolicyAdministrador)]
    [ProducesResponseType(typeof(EspetaculoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        [FromRoute] int id,
        [FromBody] EspetaculoUpdateDto body,
        [FromServices] AtualizarEspetaculoUseCase atualizar,
        CancellationToken cancellationToken)
    {
        var detalhe = await atualizar.ExecutarAsync(id, body, cancellationToken);
        return Ok(detalhe);
    }

    /// <summary>Exclui um espetáculo. Requer <c>administrador</c>.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthSetup.PolicyAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(
        [FromRoute] int id,
        [FromServices] ExcluirEspetaculoUseCase excluir,
        CancellationToken cancellationToken)
    {
        await excluir.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
