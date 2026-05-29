namespace Spotlight.Usuario.Application.Dtos;

/// <summary>Perfil do usuário — resposta de GET/PUT /usuario/me.</summary>
public class UsuarioDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly? DataNascimento { get; init; }
    public string? Localizacao { get; init; }
    public string TipoUsuario { get; init; } = string.Empty;
    public DateTime DataCadastro { get; init; }
}

/// <summary>Body de PUT /usuario/me — todos os campos opcionais (partial update).</summary>
public class UsuarioUpdateDto
{
    public string? Nome { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Localizacao { get; set; }
}
