using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Recomendacao.Presentation.Controllers;

/// <summary>
/// Controller de smoke test do serviço de recomendação (valida roteamento e Swagger).
/// As rotas reais de negócio (sugestões, ranking) entram depois.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>Retorna "pong" — usado apenas para smoke test do serviço.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "recomendacao", message = "pong" });
}
