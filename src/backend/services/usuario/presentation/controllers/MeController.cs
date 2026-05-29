using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Infrastructure.Auth;

namespace Spotlight.Usuario.Presentation.Controllers;

[ApiController]
[Authorize(Policy = AuthSetup.PolicyCadastrado)]
[Route("me")]
public sealed class MeController(IUsuarioLocalizacaoService localizacaoService) : ControllerBase
{
    [HttpPut("localizacao")]
    public async Task<ActionResult> AtualizarLocalizacao(
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken)
    {
        var idUsuario = GetUsuarioId();

        await localizacaoService.AtualizarLocalizacaoAsync(
            idUsuario,
            request,
            cancellationToken);

        return Ok(new { message = "Localizacao atualizada com sucesso." });
    }

    private Guid GetUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var idUsuario)
            ? idUsuario
            : throw new UnauthorizedAccessException("Token sem identificador de usuario valido.");
    }
}
