using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Presentation.Controllers;

/// <summary>Operacoes administrativas sobre usuarios.</summary>
[ApiController]
[Route("admin/usuarios")]
[Authorize]
public class AdminUsuariosController : ControllerBase
{
    [HttpPost("promover")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoverAdministrador(
        [FromBody] PromoverAdministradorDto body,
        [FromServices] PromoverAdministradorUseCase promover,
        [FromServices] IUsuarioContexto usuarioContexto,
        CancellationToken cancellationToken)
    {
        return Ok(await promover.ExecutarAsync(
            ExigirUsuario(usuarioContexto),
            body,
            cancellationToken));
    }

    private static Guid ExigirUsuario(IUsuarioContexto ctx)
        => ctx.UsuarioId ?? throw new ValidationException(new Dictionary<string, string[]>
            { ["sub"] = ["Claim 'sub' ausente ou invalida no token."] });
}
