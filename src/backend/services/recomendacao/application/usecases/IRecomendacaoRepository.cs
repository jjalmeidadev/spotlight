using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Application.UseCases;

public interface IRecomendacaoRepository
{
    /// <summary>
    /// Lê as recomendações pré-processadas do usuário, ordenadas por score DESC.
    /// </summary>
    Task<IReadOnlyList<Domain.Entities.Recomendacao>> ListarPorUsuarioAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);
}
