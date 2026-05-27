using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Usuario.Presentation.Controllers;

/// <summary>
/// Controller de smoke test do serviço de usuário (valida roteamento e Swagger).
/// As rotas reais de negócio (perfis, preferências, histórico) entram no T16.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>Retorna "pong" — usado apenas para smoke test do serviço.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "usuario", message = "pong" });
}
