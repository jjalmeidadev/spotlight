using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Api.Controllers;

/// <summary>
/// Controller de exemplo para validar o roteamento e o Swagger.
/// As rotas reais de negócio (catálogo, usuário, recomendação) entram no T16.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>Retorna "pong" — usado apenas para smoke test da API.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { message = "pong" });
}
