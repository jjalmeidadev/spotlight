namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um espetáculo do catálogo (visão de resumo,
/// usada em listagens). Mapeia as colunas da tabela <c>espetaculo</c>.
/// </summary>
public class Espetaculo
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Genero { get; init; }

    /// <summary>Valores possíveis: <c>em_cartaz</c>, <c>em_breve</c>, <c>encerrado</c>.</summary>
    public string Status { get; init; } = string.Empty;

    public string? FaixaEtaria { get; init; }
    public string? ClassificacaoIndicativa { get; init; }

    /// <summary>Duração em minutos.</summary>
    public int? Duracao { get; init; }

    public decimal NotaMedia { get; init; }
    public decimal Popularidade { get; init; }
    public IReadOnlyList<EspetaculoMidia> Midias { get; init; } = [];
}

/// <summary>Metadado de arquivo no Supabase Storage associado ao espetáculo.</summary>
public class EspetaculoMidia
{
    public string TipoMidia { get; init; } = string.Empty;
    public string StorageBucket { get; init; } = string.Empty;
    public string StoragePath { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int OrdemExibicao { get; init; }
}
