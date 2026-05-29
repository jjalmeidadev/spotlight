using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Acesso aos dados do próprio usuário autenticado. A implementação concreta
/// (SQL puro via Npgsql) fica em <c>infrastructure/persistence</c>.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Retorna o perfil ou <c>null</c> se o id não existe na tabela <c>usuario</c>.</summary>
    Task<Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Aplica update parcial nos campos não-nulos do DTO. Retorna <c>false</c>
    /// se o id não existe.
    /// </summary>
    Task<bool> AtualizarAsync(Guid id, UsuarioUpdateDto dados, CancellationToken cancellationToken);

    /// <summary>Lê o JSONB <c>perfil_preferencias</c> e mapeia para o domínio. <c>null</c> se id não existe.</summary>
    Task<Preferencias?> ObterPreferenciasAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Substitui o JSONB <c>perfil_preferencias</c>. Retorna <c>false</c> se o id não existe.</summary>
    Task<bool> AtualizarPreferenciasAsync(Guid id, Preferencias dados, CancellationToken cancellationToken);
}
