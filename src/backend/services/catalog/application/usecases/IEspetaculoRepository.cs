using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Porta de acesso aos dados de espetáculos. A implementação concreta
/// (SQL puro via Npgsql) fica em <c>infrastructure/persistence</c>.
/// </summary>
public interface IEspetaculoRepository
{
    /// <summary>
    /// Lista espetáculos aplicando os filtros e a paginação informados
    /// (que já chegam normalizados pelo use case).
    /// </summary>
    /// <returns>Os itens da página e o total de registros que satisfazem o filtro.</returns>
    Task<(IReadOnlyList<Espetaculo> Itens, int Total)> ListarAsync(
        ListarEspetaculosFiltro filtro, CancellationToken cancellationToken);

    /// <summary>Detalhe de um espetáculo (com categorias, atores e sessões) ou <c>null</c>.</summary>
    Task<EspetaculoDetalhe?> ObterDetalheAsync(int id, CancellationToken cancellationToken);

    /// <summary>Verifica se o espetáculo existe (usado para validar referências).</summary>
    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Cria um espetáculo (com categorias e atores), tudo numa transação.
    /// Retorna o ID gerado.
    /// </summary>
    Task<int> CriarAsync(EspetaculoCreateDto dados, CancellationToken cancellationToken);

    /// <summary>
    /// Atualiza os campos não-nulos de um espetáculo. Se <c>CategoriaIds</c> ou
    /// <c>Atores</c> forem informados (não-nulos), substituem as associações;
    /// caso contrário ficam como estão. Retorna <c>false</c> se o id não existe.
    /// </summary>
    Task<bool> AtualizarAsync(int id, EspetaculoUpdateDto dados, CancellationToken cancellationToken);

    /// <summary>Exclui um espetáculo. Retorna <c>false</c> se o id não existe.</summary>
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken);
}
