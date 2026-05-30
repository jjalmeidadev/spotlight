namespace Spotlight.Usuario.Application.Dtos;

/// <summary>Item do histórico de avaliações do usuário (com espetáculo embutido).</summary>
public class AvaliacaoUsuarioDto
{
    public int Id { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public EspetaculoResumoUsuarioDto Espetaculo { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

/// <summary>Identificação resumida de um espetáculo (id + titulo).</summary>
public class EspetaculoResumoUsuarioDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
}
