namespace Spotlight.Usuario.Domain.Entities;

/// <summary>Perfil do usuário autenticado (tabela <c>usuario</c>).</summary>
public class Usuario
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly? DataNascimento { get; init; }
    public string? Localizacao { get; init; }
    public string TipoUsuario { get; init; } = string.Empty;
    public DateTime DataCadastro { get; init; }
}

/// <summary>
/// Preferências declaradas pelo usuário (campo <c>perfil_preferencias</c> JSONB).
/// Estrutura proposta no contrato — pode evoluir.
/// </summary>
public class Preferencias
{
    public List<string> GenerosFavoritos { get; init; } = [];
    public List<int> CategoriasFavoritas { get; init; } = [];
    public decimal? FaixaPrecoMax { get; init; }
}

/// <summary>Item do histórico de avaliações do usuário (inclui o espetáculo).</summary>
public class AvaliacaoUsuario
{
    public int Id { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public DateTime CreatedAt { get; init; }
    public EspetaculoResumoUsuario Espetaculo { get; init; } = new();
}

/// <summary>Identificação resumida de um espetáculo (id + titulo) embutida em avaliações.</summary>
public class EspetaculoResumoUsuario
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
}
