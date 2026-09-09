namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>
/// Resumo de espetáculo retornado nas listagens. Espelha o schema
/// <c>Espetaculo</c> dos contratos de API (campos em camelCase no JSON).
/// </summary>
public class EspetaculoResumoDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Genero { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FaixaEtaria { get; init; }
    public string? ClassificacaoIndicativa { get; init; }
    public int? Duracao { get; init; }
    public decimal NotaMedia { get; init; }
    public decimal Popularidade { get; init; }
    public IReadOnlyList<EspetaculoMidiaDto> Midias { get; init; } = [];
}

/// <summary>Arquivo associado ao espetáculo no bucket Supabase Storage.</summary>
public class EspetaculoMidiaDto
{
    public string TipoMidia { get; init; } = string.Empty;
    public string StorageBucket { get; init; } = string.Empty;
    public string StoragePath { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int OrdemExibicao { get; init; }
}
