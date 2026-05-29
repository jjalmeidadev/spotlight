using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public interface IAvaliacaoRepository
{
    /// <summary>Lista avaliações de um espetáculo (paginadas).</summary>
    Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ListarPorEspetaculoAsync(
        int espetaculoId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Insere uma avaliação. Lança <see cref="ConflictException"/> caso o usuário
    /// já tenha avaliado este espetáculo (constraint <c>UNIQUE</c>).
    /// </summary>
    Task<Avaliacao> CriarAsync(
        int espetaculoId, Guid usuarioId, decimal nota, string? comentario,
        CancellationToken cancellationToken);
}
