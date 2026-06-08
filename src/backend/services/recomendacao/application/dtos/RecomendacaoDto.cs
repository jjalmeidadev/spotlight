namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Item retornado por <c>GET /recomendacao/me</c>.</summary>
public class RecomendacaoDto
{
    public EspetaculoRecomendadoDto Espetaculo { get; init; } = new();
    public decimal Score { get; init; }
    public string TipoAlgoritmo { get; init; } = string.Empty;
    public string? Motivo { get; init; }
    public DateTime DataGeracao { get; init; }

    /// <summary>
    /// Rótulo de cidade/região derivado da localização consentida do usuário via
    /// reverse geocoding (OpenStreetMap/Nominatim). É <c>null</c> quando o usuário
    /// não tem localização OU quando o serviço externo de geocodificação está
    /// indisponível — neste último caso o endpoint degrada graciosamente e ainda
    /// retorna as recomendações (RNF03 — Confiabilidade).
    /// </summary>
    public string? ContextoGeografico { get; init; }
}

/// <summary>Subconjunto de campos do espetáculo expostos junto da recomendação.</summary>
public class EspetaculoRecomendadoDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Genero { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal NotaMedia { get; init; }
}

/// <summary>Envelope <c>{ "data": [...] }</c> sem paginação.</summary>
public class ListaDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
}
