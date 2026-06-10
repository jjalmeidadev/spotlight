using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;

namespace Spotlight.Recomendacao.Presentation.Controllers;

/// <summary>
/// Ponto de entrada base da API de recomendacao. O API Gateway expoe esta rota
/// como <c>GET /recomendacao</c> e remove o prefixo antes de encaminhar ao servico.
/// </summary>
[ApiController]
[Route("")]
public class RecomendacaoBaseController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(RecomendacaoBaseDto), StatusCodes.Status200OK)]
    public IActionResult Obter()
    {
        return Ok(new RecomendacaoBaseDto
        {
            Endpoints =
            [
                new RecomendacaoEndpointDto
                {
                    Method = "GET",
                    Path = "/recomendacao/me",
                    Auth = "cadastrado",
                    Description = "Lista recomendacoes personalizadas do usuario autenticado."
                },
                new RecomendacaoEndpointDto
                {
                    Method = "GET",
                    Path = "/recomendacao/espetaculos/{id}/similares",
                    Auth = "publico",
                    Description = "Lista espetaculos similares a um espetaculo de origem."
                }
            ]
        });
    }
}
