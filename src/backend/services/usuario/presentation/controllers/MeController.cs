using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;

namespace Spotlight.Usuario.Presentation.Controllers;

[ApiController]
[Route("api/me")]
public sealed class MeController(IUsuarioLocalizacaoService localizacaoService) : ControllerBase
{
    [HttpPut("localizacao")]
    public async Task<IActionResult> AtualizarLocalizacao(
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await localizacaoService.AtualizarLocalizacaoAsync(
                Request.Headers.Authorization,
                request,
                cancellationToken);

            return Ok(new { message = "Localizacao atualizada com sucesso." });
        }
        catch (UsuarioValidationException ex)
        {
            return BadRequest(Error(ex.Message));
        }
        catch (UsuarioUnauthorizedException ex)
        {
            return Unauthorized(Error(ex.Message));
        }
        catch (UsuarioPersistenceException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error(ex.Message));
        }
    }

    private object Error(string message) => new
    {
        error = message,
        traceId = HttpContext.TraceIdentifier
    };
}
