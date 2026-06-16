namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>Categoria associada a espetáculos (tabela <c>categoria</c>).</summary>
public class Categoria
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
}
