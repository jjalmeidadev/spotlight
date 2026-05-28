using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Tests;

/// <summary>
/// Repositórios fake reutilizados nos testes unitários (sem banco).
/// Cada um expõe o que foi recebido para asserções e devolve dados configuráveis.
/// </summary>
internal sealed class EspetaculoRepoFake : IEspetaculoRepository
{
    public bool ExisteResultado { get; set; } = true;
    public EspetaculoDetalhe? DetalheResultado { get; set; }
    public int CriarResultado { get; set; } = 1;
    public bool AtualizarResultado { get; set; } = true;
    public bool ExcluirResultado { get; set; } = true;
    public EspetaculoCreateDto? CriarRecebido { get; private set; }
    public (int Id, EspetaculoUpdateDto Dados)? AtualizarRecebido { get; private set; }
    public int? ExcluirRecebido { get; private set; }

    public Task<(IReadOnlyList<Espetaculo> Itens, int Total)> ListarAsync(
        ListarEspetaculosFiltro filtro, CancellationToken cancellationToken)
        => Task.FromResult<(IReadOnlyList<Espetaculo>, int)>(([], 0));

    public Task<EspetaculoDetalhe?> ObterDetalheAsync(int id, CancellationToken cancellationToken)
        => Task.FromResult(DetalheResultado);

    public Task<bool> ExisteAsync(int id, CancellationToken cancellationToken)
        => Task.FromResult(ExisteResultado);

    public Task<int> CriarAsync(EspetaculoCreateDto dados, CancellationToken cancellationToken)
    {
        CriarRecebido = dados;
        return Task.FromResult(CriarResultado);
    }

    public Task<bool> AtualizarAsync(int id, EspetaculoUpdateDto dados, CancellationToken cancellationToken)
    {
        AtualizarRecebido = (id, dados);
        return Task.FromResult(AtualizarResultado);
    }

    public Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken)
    {
        ExcluirRecebido = id;
        return Task.FromResult(ExcluirResultado);
    }
}

internal sealed class AvaliacaoRepoFake : IAvaliacaoRepository
{
    public Func<int, Guid, decimal, string?, Avaliacao> CriarHandler { get; set; } =
        (esp, user, nota, com) => new Avaliacao
        {
            Id = 41, Nota = nota, Comentario = com, CreatedAt = DateTime.UtcNow,
            Usuario = new UsuarioResumo { Id = user, Nome = "Maria" }
        };

    public Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ListarPorEspetaculoAsync(
        int espetaculoId, int page, int pageSize, CancellationToken cancellationToken)
        => Task.FromResult<(IReadOnlyList<Avaliacao>, int)>(([], 0));

    public Task<Avaliacao> CriarAsync(
        int espetaculoId, Guid usuarioId, decimal nota, string? comentario,
        CancellationToken cancellationToken)
        => Task.FromResult(CriarHandler(espetaculoId, usuarioId, nota, comentario));
}
