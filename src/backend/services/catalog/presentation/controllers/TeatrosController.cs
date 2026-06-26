using Microsoft.AspNetCore.Mvc;
using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;

namespace Spotlight.Catalogo.Presentation.Controllers;

/// <summary>Rotas de teatros. Apenas leitura pública.</summary>
[ApiController]
[Route("teatros")]
public class TeatrosController : ControllerBase
{
    /// <summary>Lista todos os teatros cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListaDto<TeatroDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listar(
        [FromServices] ListarTeatrosUseCase listar, CancellationToken cancellationToken)
    {
        return Ok(await listar.ExecutarAsync(cancellationToken));
    }

    /// <summary>Detalha um teatro pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TeatroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] int id,
        [FromServices] ObterTeatroUseCase obter,
        CancellationToken cancellationToken)
    {
        return Ok(await obter.ExecutarAsync(id, cancellationToken));
    }
}
