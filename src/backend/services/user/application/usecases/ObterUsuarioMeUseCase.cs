using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Retorna o perfil do usuário autenticado. Lança <see cref="NotFoundException"/>
/// se o usuário não existir na tabela <c>usuario</c> (caso anômalo: JWT válido
/// referenciando um id que sumiu, ex.: usuário excluído).
/// </summary>
public class ObterUsuarioMeUseCase(IUsuarioRepository repositorio)
{
    public async Task<UsuarioDto> ExecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var perfil = await repositorio.ObterAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Perfil do usuário autenticado não encontrado.");

        return Mapear(perfil);
    }

    /// <summary>Reutilizado por <see cref="AtualizarUsuarioMeUseCase"/>.</summary>
    internal static UsuarioDto Mapear(Domain.Entities.Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        DataNascimento = u.DataNascimento,
        Localizacao = u.Localizacao,
        TipoUsuario = u.TipoUsuario,
        DataCadastro = u.DataCadastro
    };
}
