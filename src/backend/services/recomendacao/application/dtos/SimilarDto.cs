namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Item retornado por <c>GET /recomendacao/espetaculos/{id}/similares</c>.</summary>
public class SimilarDto
{
    public EspetaculoRecomendadoDto Espetaculo { get; init; } = new();
    public decimal Score { get; init; }
    public string TipoAlgoritmo { get; init; } = string.Empty;
}
