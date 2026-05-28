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
}
