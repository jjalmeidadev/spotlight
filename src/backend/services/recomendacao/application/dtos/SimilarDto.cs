namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Item retornado por <c>GET /recomendacao/espetaculos/{id}/similares</c>.</summary>
public class SimilarDto
{
    /// <summary>Espetáculo similar (subconjunto de campos do catálogo).</summary>
    public EspetaculoRecomendadoDto Espetaculo { get; init; } = new();

    /// <summary>Score de similaridade no intervalo [0, 1]. Quanto maior, mais similar.</summary>
    public decimal Score { get; init; }

    /// <summary>Identificador do algoritmo de similaridade (ex.: <c>cosine</c>).</summary>
    public string TipoAlgoritmo { get; init; } = string.Empty;
}
