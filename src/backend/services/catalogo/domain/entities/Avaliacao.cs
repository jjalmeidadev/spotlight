namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>Avaliação de espetáculo por um usuário (tabela <c>avaliacao</c>).</summary>
public class Avaliacao
{
    public int Id { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public DateTime CreatedAt { get; init; }
    public UsuarioResumo Usuario { get; init; } = new();
}

/// <summary>Identificação resumida do usuário (id + nome) — usada em listagens.</summary>
public class UsuarioResumo
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
}
