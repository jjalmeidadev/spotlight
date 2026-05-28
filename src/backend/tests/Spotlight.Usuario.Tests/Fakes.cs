using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Tests;

/// <summary>Repositório de usuário fake — registra chamadas e devolve dados configuráveis.</summary>
internal sealed class UsuarioRepoFake : IUsuarioRepository
{
    public Spotlight.Usuario.Domain.Entities.Usuario? PerfilResultado { get; set; }
    public bool AtualizarResultado { get; set; } = true;
    public Preferencias? PreferenciasResultado { get; set; }
    public bool AtualizarPreferenciasResultado { get; set; } = true;

    public (Guid Id, UsuarioUpdateDto Dados)? AtualizarRecebido { get; private set; }
    public (Guid Id, Preferencias Dados)? AtualizarPreferenciasRecebido { get; private set; }

    public Task<Spotlight.Usuario.Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken ct)
        => Task.FromResult(PerfilResultado);

    public Task<bool> AtualizarAsync(Guid id, UsuarioUpdateDto dados, CancellationToken ct)
    {
        AtualizarRecebido = (id, dados);
        return Task.FromResult(AtualizarResultado);
    }

    public Task<Preferencias?> ObterPreferenciasAsync(Guid id, CancellationToken ct)
        => Task.FromResult(PreferenciasResultado);

    public Task<bool> AtualizarPreferenciasAsync(Guid id, Preferencias dados, CancellationToken ct)
    {
        AtualizarPreferenciasRecebido = (id, dados);
        return Task.FromResult(AtualizarPreferenciasResultado);
    }
}

internal sealed class AvaliacaoUsuarioRepoFake : IAvaliacaoUsuarioRepository
{
    public IReadOnlyList<AvaliacaoUsuario> Itens { get; set; } = [];
    public int Total { get; set; }
    public (Guid Id, int Page, int PageSize)? Recebido { get; private set; }

    public Task<(IReadOnlyList<AvaliacaoUsuario> Itens, int Total)> ListarPorUsuarioAsync(
        Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken)
    {
        Recebido = (usuarioId, page, pageSize);
        return Task.FromResult((Itens, Total));
    }
}
