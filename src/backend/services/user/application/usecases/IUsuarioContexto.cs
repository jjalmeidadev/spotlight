namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Contrato para obter dados do usuario autenticado da requisicao corrente.
/// </summary>
public interface IUsuarioContexto
{
    Guid? UsuarioId { get; }

    string? Email { get; }

    string? Nome { get; }

    DateOnly? DataNascimento { get; }
}
