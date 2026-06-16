namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Avaliação listada no catálogo (inclui usuário).</summary>
public class AvaliacaoDto
{
    public int Id { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public UsuarioResumoDto Usuario { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

/// <summary>Identificação resumida do usuário (id UUID + nome).</summary>
public class UsuarioResumoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
}
