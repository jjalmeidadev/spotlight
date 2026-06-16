namespace Spotlight.Recomendacao.Domain.Entities;

/// <summary>
/// Predição pré-processada da tabela <c>recomendacao</c> (escrita em batch pelo
/// pipeline de ML; consumida em leitura por este serviço).
/// </summary>
public class Recomendacao
{
    public EspetaculoRecomendado Espetaculo { get; init; } = new();
    public decimal Score { get; init; }
    public string TipoAlgoritmo { get; init; } = string.Empty;
    public string? Motivo { get; init; }
    public DateTime DataGeracao { get; init; }
}

/// <summary>Subconjunto de campos do espetáculo expostos junto da recomendação.</summary>
public class EspetaculoRecomendado
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Genero { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal NotaMedia { get; init; }
    public IReadOnlyList<EspetaculoMidia> Midias { get; init; } = [];
}

/// <summary>Metadado de arquivo no Supabase Storage associado ao espetáculo recomendado.</summary>
public class EspetaculoMidia
{
    public string TipoMidia { get; init; } = string.Empty;
    public string StorageBucket { get; init; } = string.Empty;
    public string StoragePath { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int OrdemExibicao { get; init; }
}
