using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public interface ITeatroRepository
{
    Task<IReadOnlyList<Teatro>> ListarAsync(CancellationToken cancellationToken);
    Task<Teatro?> ObterAsync(int id, CancellationToken cancellationToken);
}
