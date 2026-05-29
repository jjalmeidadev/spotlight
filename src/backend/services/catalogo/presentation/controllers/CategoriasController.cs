using Microsoft.AspNetCore.Mvc;
using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;

namespace Spotlight.Catalogo.Presentation.Controllers;

/// <summary>Rotas de categorias. Apenas leitura pública.</summary>
[ApiController]
[Route("categorias")]
public class CategoriasController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListaDto<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromServices] ListarCategoriasUseCase listar, CancellationToken cancellationToken)
    {
        return Ok(await listar.ExecutarAsync(cancellationToken));
    }
}
