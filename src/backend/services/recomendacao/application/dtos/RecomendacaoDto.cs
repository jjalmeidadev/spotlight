namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Item retornado por <c>GET /recomendacao/me</c>.</summary>
public class RecomendacaoDto
{
    public EspetaculoRecomendadoDto Espetaculo { get; init; } = new();
    public decimal Score { get; init; }
    public string TipoAlgoritmo { get; init; } = string.Empty;
    public string? Motivo { get; init; }
    public DateTime DataGeracao { get; init; }
}

/// <summary>Subconjunto de campos do espetáculo expostos junto da recomendação.</summary>
public class EspetaculoRecomendadoDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Genero { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal NotaMedia { get; init; }
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

/// <summary>Envelope <c>{ "data": [...] }</c> sem paginação.</summary>
public class ListaDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
}
