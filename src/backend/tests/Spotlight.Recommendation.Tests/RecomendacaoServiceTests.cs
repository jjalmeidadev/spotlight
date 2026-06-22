using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.Services;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;
using Xunit;
using RecomendacaoEntity = Spotlight.Recomendacao.Domain.Entities.Recomendacao;

namespace Spotlight.Recomendacao.Tests;

public class RecomendacaoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task ListarPersonalizadasAsync_EnvelopaResultadoDaStrategyEmListaDto()
    {
        var strategy = new FakeStrategy
        {
            Resultado =
            [
                new RecomendacaoDto { Score = 0.9m, TipoAlgoritmo = "alg_x" }
            ]
        };
        var service = new RecomendacaoService(new FixedFactory(strategy));

        var resultado = await service.ListarPersonalizadasAsync(Guid.NewGuid(), 5, Ct);

        var item = Assert.Single(resultado.Data);
        Assert.Equal("alg_x", item.TipoAlgoritmo);
        Assert.Equal(0.9m, item.Score);
    }

    [Fact]
    public async Task ListarPersonalizadasAsync_RepassaUsuarioIdECancellationTokenParaStrategy()
    {
        var strategy = new FakeStrategy();
        var service = new RecomendacaoService(new FixedFactory(strategy));
        var usuarioId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        await service.ListarPersonalizadasAsync(usuarioId, 7, cts.Token);

        Assert.Equal(usuarioId, strategy.LastUsuarioId);
        Assert.Equal(7, strategy.LastLimite);
        Assert.Equal(cts.Token, strategy.LastCancellationToken);
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(-100, 10)]
    [InlineData(1, 1)]
    [InlineData(25, 25)]
    [InlineData(50, 50)]
    [InlineData(51, 50)]
    [InlineData(int.MaxValue, 50)]
    public async Task ListarPersonalizadasAsync_NormalizaLimite(int? entrada, int esperado)
    {
        var strategy = new FakeStrategy();
        var service = new RecomendacaoService(new FixedFactory(strategy));

        await service.ListarPersonalizadasAsync(Guid.NewGuid(), entrada, Ct);

        Assert.Equal(esperado, strategy.LastLimite);
    }

    [Fact]
    public void RecomendacaoStrategyFactory_QuandoModeloDesabilitado_RetornaStrategyPostgres()
    {
        var repositorio = new FakeRecomendacaoRepository();
        var modelo = new FakeModeloRecomendacaoClient { Habilitado = false };
        var postgres = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var mlNet = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, postgres, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);
        var factory = new RecomendacaoStrategyFactory(modelo, mlNet, postgres);

        var escolhida = factory.Criar();

        Assert.Same(postgres, escolhida);
    }

    [Fact]
    public void RecomendacaoStrategyFactory_QuandoModeloHabilitado_RetornaStrategyMlNet()
    {
        var repositorio = new FakeRecomendacaoRepository();
        var modelo = new FakeModeloRecomendacaoClient { Habilitado = true };
        var postgres = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var mlNet = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, postgres, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);
        var factory = new RecomendacaoStrategyFactory(modelo, mlNet, postgres);

        var escolhida = factory.Criar();

        Assert.Same(mlNet, escolhida);
    }

    [Fact]
    public async Task PostgresStrategy_MapeiaEntidadeParaDtoIncluindoMidias()
    {
        var espetaculo = new EspetaculoRecomendado
        {
            Id = 42,
            Titulo = "Auto da Compadecida",
            Genero = "Comedia",
            Status = "em_cartaz",
            NotaMedia = 4.8m,
            Midias =
            [
                new EspetaculoMidia
                {
                    TipoMidia = "poster",
                    StorageBucket = "espetaculos",
                    StoragePath = "auto/poster.jpg",
                    AltText = "Cartaz do espetaculo",
                    OrdemExibicao = 1
                }
            ]
        };
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes =
            [
                new RecomendacaoEntity
                {
                    Score = 0.81m,
                    TipoAlgoritmo = "content_based",
                    Motivo = "Genero favorito",
                    DataGeracao = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc),
                    Espetaculo = espetaculo
                }
            ]
        };
        var strategy = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 10, Ct);

        var item = Assert.Single(resultado);
        Assert.Equal(0.81m, item.Score);
        Assert.Equal("content_based", item.TipoAlgoritmo);
        Assert.Equal("Genero favorito", item.Motivo);
        Assert.Equal(new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc), item.DataGeracao);
        Assert.Equal(42, item.Espetaculo.Id);
        Assert.Equal("Auto da Compadecida", item.Espetaculo.Titulo);
        Assert.Equal("Comedia", item.Espetaculo.Genero);
        Assert.Equal("em_cartaz", item.Espetaculo.Status);
        Assert.Equal(4.8m, item.Espetaculo.NotaMedia);
        var midia = Assert.Single(item.Espetaculo.Midias);
        Assert.Equal("poster", midia.TipoMidia);
        Assert.Equal("espetaculos", midia.StorageBucket);
        Assert.Equal("auto/poster.jpg", midia.StoragePath);
        Assert.Equal("Cartaz do espetaculo", midia.AltText);
        Assert.Equal(1, midia.OrdemExibicao);
    }

    [Fact]
    public async Task PostgresStrategy_QuandoRepositorioRetornaListaVazia_RetornaListaVazia()
    {
        var repositorio = new FakeRecomendacaoRepository { Recomendacoes = [] };
        var strategy = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 10, Ct);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task PostgresStrategy_PreservaOrdemDoRepositorio()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes =
            [
                NovaRecomendacao(1, "Primeiro", 0.9m),
                NovaRecomendacao(2, "Segundo", 0.8m),
                NovaRecomendacao(3, "Terceiro", 0.7m)
            ]
        };
        var strategy = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 10, Ct);

        Assert.Collection(resultado,
            r => Assert.Equal("Primeiro", r.Espetaculo.Titulo),
            r => Assert.Equal("Segundo", r.Espetaculo.Titulo),
            r => Assert.Equal("Terceiro", r.Espetaculo.Titulo));
    }

    [Fact]
    public async Task MlNetStrategy_QuandoModeloRetornaPredicoesEEspetaculosExistem_UsaRankingDoModelo()
    {
        var antes = DateTime.UtcNow;
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>
            {
                [10] = new() { Id = 10, Titulo = "Hamlet" },
                [11] = new() { Id = 11, Titulo = "Macbeth" }
            }
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes =
            [
                new ModeloRecomendacaoPredicao(11, 0.95m, "mlnet", "vizinho do favorito"),
                new ModeloRecomendacaoPredicao(10, 0.80m, "mlnet", null)
            ]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        Assert.Equal(0, repositorio.ListarPorUsuarioCalls);
        Assert.Equal([11, 10], repositorio.LastEspetaculoIds);
        Assert.Collection(resultado,
            primeiro =>
            {
                Assert.Equal("Macbeth", primeiro.Espetaculo.Titulo);
                Assert.Equal(0.95m, primeiro.Score);
                Assert.Equal("mlnet", primeiro.TipoAlgoritmo);
                Assert.Equal("vizinho do favorito", primeiro.Motivo);
                Assert.InRange(primeiro.DataGeracao, antes, DateTime.UtcNow);
            },
            segundo =>
            {
                Assert.Equal("Hamlet", segundo.Espetaculo.Titulo);
                Assert.Null(segundo.Motivo);
            });
    }

    [Fact]
    public async Task MlNetStrategy_RepassaUsuarioLimiteECancellationTokenParaModelo()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>
            {
                [10] = new() { Id = 10, Titulo = "Hamlet" }
            }
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes = [new ModeloRecomendacaoPredicao(10, 0.80m, "mlnet", null)]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);
        var usuarioId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        await strategy.ListarAsync(usuarioId, 3, cts.Token);

        Assert.Equal(usuarioId, modelo.LastUsuarioId);
        Assert.Equal(3, modelo.LastLimite);
        Assert.Equal(cts.Token, modelo.LastCancellationToken);
    }

    [Fact]
    public async Task MlNetStrategy_QuandoRepositorioFalhaAoEnriquecer_FazFallbackELogaWarning()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            ExcecaoAoObterEspetaculos = new InvalidOperationException("postgres indisponivel"),
            Recomendacoes = [NovaRecomendacao(7, "Fallback apos falha no enriquecimento", 0.6m)]
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes = [new ModeloRecomendacaoPredicao(20, 0.91m, "mlnet", null)]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var logger = new CapturingLogger<ModeloMlNetRecomendacaoStrategy>();
        var strategy = new ModeloMlNetRecomendacaoStrategy(repositorio, modelo, fallback, logger);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        Assert.Equal("Fallback apos falha no enriquecimento", Assert.Single(resultado).Espetaculo.Titulo);
        Assert.Equal(1, repositorio.ListarPorUsuarioCalls);
        var warning = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, warning.Level);
        Assert.IsType<InvalidOperationException>(warning.Exception);
    }

    [Fact]
    public async Task MlNetStrategy_ComPredicoesDuplicadas_PreservaRankingDoModelo()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>
            {
                [10] = new() { Id = 10, Titulo = "Hamlet" },
                [11] = new() { Id = 11, Titulo = "Macbeth" }
            }
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes =
            [
                new ModeloRecomendacaoPredicao(10, 0.95m, "mlnet", "primeira ocorrencia"),
                new ModeloRecomendacaoPredicao(11, 0.90m, "mlnet", null),
                new ModeloRecomendacaoPredicao(10, 0.70m, "mlnet", "segunda ocorrencia")
            ]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 3, Ct);

        Assert.Equal(["Hamlet", "Macbeth", "Hamlet"], resultado.Select(item => item.Espetaculo.Titulo));
        Assert.Equal([0.95m, 0.90m, 0.70m], resultado.Select(item => item.Score));
    }

    [Fact]
    public async Task MlNetStrategy_QuandoModeloRetornaPredicoesVazias_FazFallbackParaPostgres()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes = [NovaRecomendacao(99, "Fallback", 0.4m)]
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes = []
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        var item = Assert.Single(resultado);
        Assert.Equal("Fallback", item.Espetaculo.Titulo);
        Assert.Equal(1, repositorio.ListarPorUsuarioCalls);
    }

    [Fact]
    public async Task MlNetStrategy_QuandoNenhumaPredicaoTemEspetaculoConhecido_FazFallbackParaPostgres()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>(),
            Recomendacoes = [NovaRecomendacao(7, "Fallback Postgres", 0.5m)]
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes =
            [
                new ModeloRecomendacaoPredicao(1, 0.99m, "mlnet", null),
                new ModeloRecomendacaoPredicao(2, 0.88m, "mlnet", null)
            ]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        var item = Assert.Single(resultado);
        Assert.Equal("Fallback Postgres", item.Espetaculo.Titulo);
        Assert.Equal(1, repositorio.ListarPorUsuarioCalls);
    }

    [Fact]
    public async Task MlNetStrategy_FiltraPredicoesSemEspetaculoMasMantemDemais()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            EspetaculosPorId = new Dictionary<int, EspetaculoRecomendado>
            {
                [20] = new() { Id = 20, Titulo = "Disponivel" }
            }
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            Predicoes =
            [
                new ModeloRecomendacaoPredicao(20, 0.91m, "mlnet", null),
                new ModeloRecomendacaoPredicao(404, 0.80m, "mlnet", "Sem espetaculo no postgres")
            ]
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var strategy = new ModeloMlNetRecomendacaoStrategy(
            repositorio, modelo, fallback, NullLogger<ModeloMlNetRecomendacaoStrategy>.Instance);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        var item = Assert.Single(resultado);
        Assert.Equal("Disponivel", item.Espetaculo.Titulo);
        Assert.Equal(0, repositorio.ListarPorUsuarioCalls);
    }

    [Fact]
    public async Task MlNetStrategy_QuandoModeloLancaExcecao_FazFallbackELogaWarning()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes = [NovaRecomendacao(7, "Fallback apos erro", 0.6m)]
        };
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            ExcecaoAoPredizer = new InvalidOperationException("modelo offline")
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var logger = new CapturingLogger<ModeloMlNetRecomendacaoStrategy>();
        var strategy = new ModeloMlNetRecomendacaoStrategy(repositorio, modelo, fallback, logger);

        var resultado = await strategy.ListarAsync(Guid.NewGuid(), 5, Ct);

        var item = Assert.Single(resultado);
        Assert.Equal("Fallback apos erro", item.Espetaculo.Titulo);
        var warning = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, warning.Level);
        Assert.IsType<InvalidOperationException>(warning.Exception);
    }

    [Fact]
    public async Task MlNetStrategy_QuandoModeloLancaOperationCanceledException_NaoFazFallbackEPropaga()
    {
        var repositorio = new FakeRecomendacaoRepository
        {
            Recomendacoes = [NovaRecomendacao(7, "Nunca chamado", 0.6m)]
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var modelo = new FakeModeloRecomendacaoClient
        {
            Habilitado = true,
            ExcecaoAoPredizer = new OperationCanceledException(cts.Token)
        };
        var fallback = new PostgresPreProcessadoRecomendacaoStrategy(repositorio);
        var logger = new CapturingLogger<ModeloMlNetRecomendacaoStrategy>();
        var strategy = new ModeloMlNetRecomendacaoStrategy(repositorio, modelo, fallback, logger);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            strategy.ListarAsync(Guid.NewGuid(), 5, cts.Token));

        Assert.Equal(0, repositorio.ListarPorUsuarioCalls);
        Assert.Empty(logger.Entries);
    }

    private static RecomendacaoEntity NovaRecomendacao(int id, string titulo, decimal score) => new()
    {
        Score = score,
        TipoAlgoritmo = "stub",
        DataGeracao = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc),
        Espetaculo = new EspetaculoRecomendado { Id = id, Titulo = titulo, Status = "em_cartaz" }
    };

    private sealed class FakeStrategy : IRecomendacaoStrategy
    {
        public IReadOnlyList<RecomendacaoDto> Resultado { get; init; } = [];
        public Guid LastUsuarioId { get; private set; }
        public int LastLimite { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
            Guid usuarioId, int limite, CancellationToken cancellationToken)
        {
            LastUsuarioId = usuarioId;
            LastLimite = limite;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(Resultado);
        }
    }

    private sealed class FixedFactory(IRecomendacaoStrategy strategy) : IRecomendacaoStrategyFactory
    {
        public IRecomendacaoStrategy Criar() => strategy;
    }

    private sealed class FakeRecomendacaoRepository : IRecomendacaoRepository
    {
        public IReadOnlyList<RecomendacaoEntity> Recomendacoes { get; set; } = [];
        public IReadOnlyDictionary<int, EspetaculoRecomendado> EspetaculosPorId { get; set; } =
            new Dictionary<int, EspetaculoRecomendado>();
        public Exception? ExcecaoAoObterEspetaculos { get; init; }
        public int ListarPorUsuarioCalls { get; private set; }
        public int[] LastEspetaculoIds { get; private set; } = [];

        public Task<IReadOnlyList<RecomendacaoEntity>> ListarPorUsuarioAsync(
            Guid usuarioId, int limite, CancellationToken cancellationToken)
        {
            ListarPorUsuarioCalls++;
            return Task.FromResult(Recomendacoes);
        }

        public Task<IReadOnlyDictionary<int, EspetaculoRecomendado>> ObterEspetaculosPorIdsAsync(
            IEnumerable<int> espetaculoIds, CancellationToken cancellationToken)
        {
            if (ExcecaoAoObterEspetaculos is not null)
            {
                return Task.FromException<IReadOnlyDictionary<int, EspetaculoRecomendado>>(
                    ExcecaoAoObterEspetaculos);
            }
            LastEspetaculoIds = espetaculoIds.ToArray();
            return Task.FromResult(EspetaculosPorId);
        }
    }

    private sealed class FakeModeloRecomendacaoClient : IModeloRecomendacaoClient
    {
        public bool Habilitado { get; init; }
        public IReadOnlyList<ModeloRecomendacaoPredicao> Predicoes { get; init; } = [];
        public Exception? ExcecaoAoPredizer { get; init; }
        public Guid LastUsuarioId { get; private set; }
        public int LastLimite { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<IReadOnlyList<ModeloRecomendacaoPredicao>> PredizerAsync(
            Guid usuarioId, int limite, CancellationToken cancellationToken)
        {
            LastUsuarioId = usuarioId;
            LastLimite = limite;
            LastCancellationToken = cancellationToken;
            if (ExcecaoAoPredizer is not null)
            {
                return Task.FromException<IReadOnlyList<ModeloRecomendacaoPredicao>>(ExcecaoAoPredizer);
            }
            return Task.FromResult(Predicoes);
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception), exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
