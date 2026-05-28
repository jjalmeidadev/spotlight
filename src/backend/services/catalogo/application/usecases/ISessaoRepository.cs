using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public interface ISessaoRepository
{
    Task<IReadOnlyList<Sessao>> ListarPorEspetaculoAsync(int espetaculoId, CancellationToken cancellationToken);
}
