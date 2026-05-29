using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

public interface IAvaliacaoUsuarioRepository
{
    Task<(IReadOnlyList<AvaliacaoUsuario> Itens, int Total)> ListarPorUsuarioAsync(
        Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken);
}
