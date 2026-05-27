using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Gateway.Controllers;

/// <summary>
/// Controller de smoke test do gateway (valida roteamento e Swagger).
/// O roteamento real para os serviços é feito pelo YARP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>Retorna "pong" — usado apenas para smoke test do gateway.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "gateway", message = "pong" });
}
