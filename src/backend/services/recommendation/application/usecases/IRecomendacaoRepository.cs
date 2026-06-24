using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Application.UseCases;

public interface IRecomendacaoRepository
{
    /// <summary>
    /// Lê as recomendações pré-processadas do usuário, ordenadas por score DESC.
    /// </summary>
    Task<IReadOnlyList<Domain.Entities.Recomendacao>> ListarPorUsuarioAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);

    /// <summary>
    /// Calcula recomendacoes colaborativas em tempo de leitura usando avaliacoes
    /// de usuarios com gostos semelhantes ao usuario autenticado.
    /// </summary>
    Task<IReadOnlyList<Domain.Entities.Recomendacao>> ListarUserBasedPorAvaliacoesAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);

    /// <summary>
    /// Busca os espetaculos recomendados por identificador para enriquecer a
    /// saida ranqueada pelo modelo incrementado.
    /// </summary>
    Task<IReadOnlyDictionary<int, EspetaculoRecomendado>> ObterEspetaculosPorIdsAsync(
        IEnumerable<int> espetaculoIds,
        CancellationToken cancellationToken);
}
