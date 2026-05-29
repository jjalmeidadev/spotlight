namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Contrato para obter o usuário autenticado da requisição corrente.
/// A implementação lê os claims do JWT validado pelo middleware de autenticação.
/// </summary>
public interface IUsuarioContexto
{
    /// <summary>UUID do usuário (claim <c>sub</c>) ou <c>null</c> se não autenticado.</summary>
    Guid? UsuarioId { get; }
}
