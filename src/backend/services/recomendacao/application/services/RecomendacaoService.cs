using Microsoft.Extensions.Logging;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Application.Services;

public interface IRecomendacaoService
{
    Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken);
}

/// <summary>
/// Centraliza a regra de negocio de recomendacao personalizada e delega a
/// escolha do algoritmo para uma factory de strategies.
/// </summary>
public sealed class RecomendacaoService(
    IRecomendacaoStrategyFactory strategyFactory) : IRecomendacaoService
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken)
    {
        var limiteNormalizado = NormalizarLimite(limite);
        var strategy = strategyFactory.Criar();
        var recomendacoes = await strategy.ListarAsync(usuarioId, limiteNormalizado, cancellationToken);

        return new ListaDto<RecomendacaoDto> { Data = recomendacoes };
    }

    private static int NormalizarLimite(int? limite) => limite switch
    {
        null or < 1 => LimitePadrao,
        > LimiteMaximo => LimiteMaximo,
        _ => limite.Value
    };
}

/// <summary>
/// Strategy comportamental para trocar o algoritmo de recomendacao sem alterar
/// controller ou use case.
/// </summary>
public interface IRecomendacaoStrategy
{
    Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken);
}

/// <summary>
/// Factory criacional responsavel por selecionar a strategy ativa de
/// recomendacao conforme configuracao e disponibilidade dos artefatos.
/// </summary>
public interface IRecomendacaoStrategyFactory
{
    IRecomendacaoStrategy Criar();
}

public sealed class RecomendacaoStrategyFactory(
    IModeloRecomendacaoClient modeloClient,
    ModeloMlNetRecomendacaoStrategy modeloStrategy,
    PostgresPreProcessadoRecomendacaoStrategy postgresStrategy)
    : IRecomendacaoStrategyFactory
{
    public IRecomendacaoStrategy Criar() =>
        modeloClient.Habilitado ? modeloStrategy : postgresStrategy;
}

public sealed class ModeloMlNetRecomendacaoStrategy(
    IRecomendacaoRepository repositorio,
    IModeloRecomendacaoClient modeloClient,
    PostgresPreProcessadoRecomendacaoStrategy fallbackStrategy,
    ILogger<ModeloMlNetRecomendacaoStrategy> logger) : IRecomendacaoStrategy
{
    public async Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken)
    {
        try
        {
            var predicoes = await modeloClient.PredizerAsync(usuarioId, limite, cancellationToken);
            if (predicoes.Count == 0)
            {
                return await fallbackStrategy.ListarAsync(usuarioId, limite, cancellationToken);
            }

            var espetaculos = await repositorio.ObterEspetaculosPorIdsAsync(
                predicoes.Select(predicao => predicao.ItemId),
                cancellationToken);

            var recomendacoes = predicoes
                .Where(predicao => espetaculos.ContainsKey(predicao.ItemId))
                .Select(predicao => new RecomendacaoDto
                {
                    Score = predicao.Score,
                    TipoAlgoritmo = predicao.TipoAlgoritmo,
                    Motivo = predicao.Motivo,
                    DataGeracao = DateTime.UtcNow,
                    Espetaculo = MapearEspetaculo(espetaculos[predicao.ItemId])
                })
                .ToList();

            return recomendacoes.Count > 0
                ? recomendacoes
                : await fallbackStrategy.ListarAsync(usuarioId, limite, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Falha ao executar strategy ML.NET; usando strategy PostgreSQL.");
            return await fallbackStrategy.ListarAsync(usuarioId, limite, cancellationToken);
        }
    }

    private static EspetaculoRecomendadoDto MapearEspetaculo(Domain.Entities.EspetaculoRecomendado espetaculo) => new()
    {
        Id = espetaculo.Id,
        Titulo = espetaculo.Titulo,
        Genero = espetaculo.Genero,
        Status = espetaculo.Status,
        NotaMedia = espetaculo.NotaMedia,
        Midias = espetaculo.Midias.Select(MapearMidia).ToList()
    };

    private static EspetaculoMidiaDto MapearMidia(Domain.Entities.EspetaculoMidia midia) => new()
    {
        TipoMidia = midia.TipoMidia,
        StorageBucket = midia.StorageBucket,
        StoragePath = midia.StoragePath,
        AltText = midia.AltText,
        OrdemExibicao = midia.OrdemExibicao
    };
}

public sealed class PostgresPreProcessadoRecomendacaoStrategy(
    IRecomendacaoRepository repositorio) : IRecomendacaoStrategy
{
    public async Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken)
    {
        var itens = await repositorio.ListarPorUsuarioAsync(usuarioId, limite, cancellationToken);
        return itens.Select(Mapear).ToList();
    }

    private static RecomendacaoDto Mapear(Domain.Entities.Recomendacao recomendacao) => new()
    {
        Score = recomendacao.Score,
        TipoAlgoritmo = recomendacao.TipoAlgoritmo,
        Motivo = recomendacao.Motivo,
        DataGeracao = recomendacao.DataGeracao,
        Espetaculo = new EspetaculoRecomendadoDto
            {
                Id = recomendacao.Espetaculo.Id,
                Titulo = recomendacao.Espetaculo.Titulo,
                Genero = recomendacao.Espetaculo.Genero,
                Status = recomendacao.Espetaculo.Status,
                NotaMedia = recomendacao.Espetaculo.NotaMedia,
                Midias = recomendacao.Espetaculo.Midias.Select(MapearMidia).ToList()
            }
        };

    private static EspetaculoMidiaDto MapearMidia(Domain.Entities.EspetaculoMidia midia) => new()
    {
        TipoMidia = midia.TipoMidia,
        StorageBucket = midia.StorageBucket,
        StoragePath = midia.StoragePath,
        AltText = midia.AltText,
        OrdemExibicao = midia.OrdemExibicao
    };
}

public interface IModeloRecomendacaoClient
{
    bool Habilitado { get; }

    Task<IReadOnlyList<ModeloRecomendacaoPredicao>> PredizerAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken);
}

public sealed record ModeloRecomendacaoPredicao(
    int ItemId,
    decimal Score,
    string TipoAlgoritmo,
    string? Motivo);
