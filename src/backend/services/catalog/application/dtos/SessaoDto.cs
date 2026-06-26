namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Sessão exposta nas listagens (inclui resumo do teatro).</summary>
public class SessaoDto
{
    public int Id { get; init; }

    /// <summary>Formato ISO 8601 date: <c>YYYY-MM-DD</c>.</summary>
    public string Data { get; init; } = string.Empty;

    /// <summary>Formato <c>HH:mm</c>.</summary>
    public string Horario { get; init; } = string.Empty;

    public decimal? PrecoMinimo { get; init; }
    public decimal? PrecoMaximo { get; init; }
    public int Disponibilidade { get; init; }
    public string? LinkCompra { get; init; }
    public TeatroResumoDto Teatro { get; init; } = new();
}
