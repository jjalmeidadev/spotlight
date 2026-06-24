using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Acesso aos dados do proprio usuario autenticado.
/// </summary>
public interface IUsuarioRepository
{
    Task<Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken);

    Task GarantirPerfilAsync(
        Guid id,
        string email,
        string? nome,
        DateOnly? dataNascimento,
        CancellationToken cancellationToken);

    Task<bool> AtualizarAsync(Guid id, UsuarioUpdateDto dados, CancellationToken cancellationToken);

    Task<Domain.Entities.Usuario?> PromoverAdministradorPorEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Preferencias?> ObterPreferenciasAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> AtualizarPreferenciasAsync(Guid id, Preferencias dados, CancellationToken cancellationToken);
}
