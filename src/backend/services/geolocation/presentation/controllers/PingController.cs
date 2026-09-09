using Microsoft.AspNetCore.Mvc;

namespace Spotlight.Geolocalizacao.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "geolocalizacao", message = "pong" });
}
