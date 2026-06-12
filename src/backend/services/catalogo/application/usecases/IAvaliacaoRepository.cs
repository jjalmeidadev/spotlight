using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public interface IAvaliacaoRepository
{
    /// <summary>Lista avaliacoes de um espetaculo, com paginacao.</summary>
    Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ListarPorEspetaculoAsync(
        int espetaculoId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Salva a avaliacao do usuario para um espetaculo. Se ja existir avaliacao
    /// para o mesmo par usuario/espetaculo, atualiza a nota e o comentario.
    /// </summary>
    Task<Avaliacao> CriarAsync(
        int espetaculoId, Guid usuarioId, decimal nota, string? comentario,
        CancellationToken cancellationToken);
}
