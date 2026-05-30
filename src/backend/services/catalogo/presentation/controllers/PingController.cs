using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Catalogo.Presentation.Controllers;

/// <summary>
/// Controller de smoke test do serviço de catálogo (valida roteamento e Swagger).
/// As rotas reais de negócio (espetáculos, teatros, gêneros) entram depois.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>Retorna "pong" — usado apenas para smoke test do serviço.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "catalogo", message = "pong" });
}
