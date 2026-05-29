namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>
/// Visão detalhada de um espetáculo (todos os campos + relacionamentos).
/// Usada pela rota <c>GET /catalogo/espetaculos/{id}</c>.
/// </summary>
public class EspetaculoDetalhe
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public string? Genero { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FaixaEtaria { get; init; }
    public string? ClassificacaoIndicativa { get; init; }
    public int? Duracao { get; init; }
    public string? Produtora { get; init; }
    public decimal NotaMedia { get; init; }
    public decimal Popularidade { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<CategoriaResumo> Categorias { get; init; } = [];
    public IReadOnlyList<AtorElenco> Atores { get; init; } = [];
    public IReadOnlyList<Sessao> Sessoes { get; init; } = [];
}

/// <summary>Categoria resumida (id + nome) — usada dentro de detalhes de espetáculo.</summary>
public class CategoriaResumo
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
}

/// <summary>Ator num elenco — inclui o papel e a ordem de exibição do JOIN <c>espetaculo_ator</c>.</summary>
public class AtorElenco
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Papel { get; init; }
    public int? OrdemExibicao { get; init; }
}
