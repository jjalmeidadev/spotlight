namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Detalhe completo de espetáculo (com relacionamentos).</summary>
public class EspetaculoDetalheDto
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

    public IReadOnlyList<CategoriaResumoDto> Categorias { get; init; } = [];
    public IReadOnlyList<AtorResumoDto> Atores { get; init; } = [];
    public IReadOnlyList<SessaoDto> Sessoes { get; init; } = [];
    public IReadOnlyList<EspetaculoMidiaDto> Midias { get; init; } = [];

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>Ator dentro do elenco de um espetáculo (com papel e ordem de exibição).</summary>
public class AtorResumoDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Papel { get; init; }
    public int? OrdemExibicao { get; init; }
}
