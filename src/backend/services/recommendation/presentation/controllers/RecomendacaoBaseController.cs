using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;

namespace Spotlight.Recomendacao.Presentation.Controllers;

/// <summary>
/// Ponto de entrada base da API de recomendacao. O API Gateway expoe esta rota
/// como <c>GET /recomendacao</c> e remove o prefixo antes de encaminhar ao servico.
/// </summary>
[ApiController]
[Route("")]
[Produces("application/json")]
public class RecomendacaoBaseController : ControllerBase
{
    /// <summary>
    /// Retorna o catálogo de endpoints públicos da API de recomendação (descoberta).
    /// </summary>
    /// <remarks>
    /// Contrato público: <c>GET /recomendacao</c>. Útil para clientes
    /// descobrirem dinamicamente as rotas e seus requisitos de autenticação,
    /// sem precisar consultar o Swagger.
    /// </remarks>
    /// <response code="200">Metadados do serviço e lista de endpoints expostos.</response>
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
