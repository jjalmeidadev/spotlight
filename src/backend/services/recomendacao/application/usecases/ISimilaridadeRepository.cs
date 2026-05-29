using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Application.UseCases;

public interface ISimilaridadeRepository
{
    /// <summary>Indica se um espetáculo existe na tabela <c>espetaculo</c>.</summary>
    Task<bool> EspetaculoExisteAsync(int espetaculoId, CancellationToken cancellationToken);

    /// <summary>
    /// Lê os similares globais de um espetáculo de origem, ordenados por
    /// <c>score_similaridade DESC</c>.
    /// </summary>
    Task<IReadOnlyList<Similar>> ListarPorOrigemAsync(
        int espetaculoOrigemId, int limite, CancellationToken cancellationToken);
}
