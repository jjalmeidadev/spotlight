namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>
/// Sessão de um espetáculo num teatro (tabela <c>sessao</c>). Inclui um resumo do
/// teatro (id/nome/cidade/estado) pois é como o contrato a expõe.
/// </summary>
public class Sessao
{
    public int Id { get; init; }
    public DateOnly Data { get; init; }
    public TimeOnly Horario { get; init; }
    public decimal? PrecoMinimo { get; init; }
    public decimal? PrecoMaximo { get; init; }
    public int Disponibilidade { get; init; }
    public string? LinkCompra { get; init; }
    public TeatroResumo Teatro { get; init; } = new();
}

/// <summary>Identificação resumida de um teatro (campos usados em listagens).</summary>
public class TeatroResumo
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}
