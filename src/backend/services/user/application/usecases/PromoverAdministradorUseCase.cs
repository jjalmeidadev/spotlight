using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>Promove um usuário cadastrado ao papel administrativo.</summary>
public class PromoverAdministradorUseCase(IUsuarioRepository repositorio)
{
    public async Task<UsuarioDto> ExecutarAsync(
        Guid administradorId,
        PromoverAdministradorDto dados,
        CancellationToken cancellationToken)
    {
        var administrador = await repositorio.ObterAsync(administradorId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Perfil administrativo nao encontrado.");

        if (administrador.TipoUsuario != "administrador")
        {
            throw new ForbiddenException("Usuario sem permissao para promover administradores.");
        }

        var email = dados.Email.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["email"] = ["Informe um e-mail valido para promover."]
            });
        }

        var usuario = await repositorio.PromoverAdministradorPorEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException("Usuario cadastrado nao encontrado para este e-mail.");

        return ObterUsuarioMeUseCase.Mapear(usuario);
    }
}
