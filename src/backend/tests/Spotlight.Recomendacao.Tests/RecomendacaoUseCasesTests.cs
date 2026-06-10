using Microsoft.Extensions.Logging.Abstractions;
using Spotlight.Recomendacao.Application.Services;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;
using Xunit;
using RecomendacaoEntity = Spotlight.Recomendacao.Domain.Entities.Recomendacao;
using RecomendacaoNotFoundException = Spotlight.Recomendacao.Domain.Entities.NotFoundException;

namespace Spotlight.Recomendacao.Tests;

public class RecomendacaoUseCasesTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    [InlineData(20, 20)]
    [InlineData(999, 50)]
    public async Task ListarRecomendacoesMe_NormalizaLimiteEMapeiaResultado(int? limite, int limiteEsperado)
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes =
            [
                new RecomendacaoEntity
                {
                    Score = 0.92m,
                    TipoAlgoritmo = "content_based",
                    Motivo = "Genero favorito",
                    DataGeracao = new DateTime(2026, 5, 29, 10, 0, 0, DateTimeKind.Utc),
                    Espetaculo = Espetaculo()
                }
            ]
        };
        var usuarioId = Guid.NewGuid();
        var service = new RecomendacaoService(
            repositorio,
            new FakeModeloRecomendacaoClient(),
            NullLogger<RecomendacaoService>.Instance);
        var useCase = new ListarRecomendacoesMeUseCase(service);

        var resultado = await useCase.ExecutarAsync(usuarioId, limite, Ct);

        Assert.Equal(usuarioId, repositorio.LastUsuarioId);
        Assert.Equal(limiteEsperado, repositorio.LastLimite);
        var item = Assert.Single(resultado.Data);
        Assert.Equal("content_based", item.TipoAlgoritmo);
        Assert.Equal("Hamlet", item.Espetaculo.Titulo);
        Assert.Equal(0.92m, item.Score);
    }

    [Fact]
    public async Task ListarRecomendacoesMe_QuandoModeloHabilitado_UsaRankingDoModeloEEnriqueceNoPostgres()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>
            {
                [7] = Espetaculo(),
                [9] = new()
                {
                    Id = 9,
                    Titulo = "O Auto da Compadecida",
                    Genero = "Comedia",
                    Status = "em_cartaz",
                    NotaMedia = 4.7m
                }
            }
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes =
            [
                new ModeloRecomendacaoPredicao(9, 0.98m, "hibrido_svd_knn", "Vizinho de item favorito"),
                new ModeloRecomendacaoPredicao(7, 0.95m, "hibrido_svd", "Perfil do usuario")
            ]
        };
        var service = new RecomendacaoService(repositorio, modelo, NullLogger<RecomendacaoService>.Instance);
        var useCase = new ListarRecomendacoesMeUseCase(service);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), 5, Ct);

        Assert.Equal(0, repositorio.ListarPorUsuarioCalls);
        Assert.Equal([9, 7], repositorio.LastEspetaculoIds);
        Assert.Collection(
            resultado.Data,
            primeiro =>
            {
                Assert.Equal("O Auto da Compadecida", primeiro.Espetaculo.Titulo);
                Assert.Equal("hibrido_svd_knn", primeiro.TipoAlgoritmo);
                Assert.Equal(0.98m, primeiro.Score);
            },
            segundo => Assert.Equal("Hamlet", segundo.Espetaculo.Titulo));
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 10)]
    [InlineData(25, 25)]
    [InlineData(51, 50)]
    public async Task ListarSimilares_QuandoOrigemExiste_NormalizaLimiteEMapeiaResultado(int? limite, int limiteEsperado)
    {
        var repositorio = new FakeSimilaridadeRepository
        {
            Existe = true,
            Similares =
            [
                new Similar
                {
                    Score = 0.87m,
                    TipoAlgoritmo = "item_item_knn",
                    Espetaculo = Espetaculo()
                }
            ]
        };
        var useCase = new ListarSimilaresUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(7, limite, Ct);

        Assert.Equal(7, repositorio.LastExisteId);
        Assert.Equal(7, repositorio.LastOrigemId);
        Assert.Equal(limiteEsperado, repositorio.LastLimite);
        var item = Assert.Single(resultado.Data);
        Assert.Equal("item_item_knn", item.TipoAlgoritmo);
        Assert.Equal("Hamlet", item.Espetaculo.Titulo);
    }

    [Fact]
    public async Task ListarSimilares_QuandoOrigemNaoExiste_LancaNotFoundENaoLista()
    {
        var repositorio = new FakeSimilaridadeRepository { Existe = false };
        var useCase = new ListarSimilaresUseCase(repositorio);

        await Assert.ThrowsAsync<RecomendacaoNotFoundException>(() => useCase.ExecutarAsync(404, 10, Ct));

        Assert.Equal(0, repositorio.ListarCalls);
    }

    private static EspetaculoRecomendado Espetaculo() => new()
    {
        Id = 7,
        Titulo = "Hamlet",
        Genero = "Drama",
        Status = "em_cartaz",
        NotaMedia = 4.9m
    };

    private sealed class FakeRecomendacaoRepository : IRecomendacaoRepository
    {
        public IReadOnlyList<RecomendacaoEntity> Recomendacoes { get; set; } = [];
        public IReadOnlyDictionary<int, EspetaculoRecomendado> EspetaculosPorId { get; set; } =
            new Dictionary<int, EspetaculoRecomendado>();
        public Guid LastUsuarioId { get; private set; }
        public int LastLimite { get; private set; }
        public int ListarPorUsuarioCalls { get; private set; }
        public int[] LastEspetaculoIds { get; private set; } = [];

        public Task<IReadOnlyList<RecomendacaoEntity>> ListarPorUsuarioAsync(
            Guid usuarioId,
            int limite,
            CancellationToken cancellationToken)
        {
            ListarPorUsuarioCalls++;
            LastUsuarioId = usuarioId;
            LastLimite = limite;
            return Task.FromResult(Recomendacoes);
        }

        public Task<IReadOnlyDictionary<int, EspetaculoRecomendado>> ObterEspetaculosPorIdsAsync(
            IEnumerable<int> espetaculoIds,
            CancellationToken cancellationToken)
        {
            LastEspetaculoIds = espetaculoIds.ToArray();
            return Task.FromResult(EspetaculosPorId);
        }
    }

    private sealed class FakeModeloRecomendacaoClient : IModeloRecomendacaoClient
    {
        public bool Habilitado { get; init; }
        public IReadOnlyList<ModeloRecomendacaoPredicao> Predicoes { get; init; } = [];

        public Task<IReadOnlyList<ModeloRecomendacaoPredicao>> PredizerAsync(
            Guid usuarioId,
            int limite,
            CancellationToken cancellationToken) => Task.FromResult(Predicoes);
    }

    private sealed class FakeSimilaridadeRepository : ISimilaridadeRepository
    {
        public bool Existe { get; set; }
        public IReadOnlyList<Similar> Similares { get; set; } = [];
        public int LastExisteId { get; private set; }
        public int LastOrigemId { get; private set; }
        public int LastLimite { get; private set; }
        public int ListarCalls { get; private set; }

        public Task<bool> EspetaculoExisteAsync(int espetaculoId, CancellationToken cancellationToken)
        {
            LastExisteId = espetaculoId;
            return Task.FromResult(Existe);
        }

        public Task<IReadOnlyList<Similar>> ListarPorOrigemAsync(
            int espetaculoOrigemId,
            int limite,
            CancellationToken cancellationToken)
        {
            ListarCalls++;
            LastOrigemId = espetaculoOrigemId;
            LastLimite = limite;
            return Task.FromResult(Similares);
        }
    }
}
