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

public sealed class RecomendacaoService(
    IRecomendacaoRepository repositorio,
    IModeloRecomendacaoClient modeloClient,
    ILogger<RecomendacaoService> logger) : IRecomendacaoService
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken)
    {
        var limiteNorm = NormalizarLimite(limite);
        var recomendacoesModelo = await TentarListarPorModeloAsync(usuarioId, limiteNorm, cancellationToken);

        if (recomendacoesModelo.Count > 0)
        {
            return new ListaDto<RecomendacaoDto> { Data = recomendacoesModelo };
        }

        var recomendacoes = await repositorio.ListarPorUsuarioAsync(usuarioId, limiteNorm, cancellationToken);
        return new ListaDto<RecomendacaoDto> { Data = recomendacoes.Select(Mapear).ToList() };
    }

    private async Task<IReadOnlyList<RecomendacaoDto>> TentarListarPorModeloAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken)
    {
        if (!modeloClient.Habilitado)
        {
            return [];
        }

        try
        {
            var predicoes = await modeloClient.PredizerAsync(usuarioId, limite, cancellationToken);
            if (predicoes.Count == 0)
            {
                return [];
            }

            var espetaculos = await repositorio.ObterEspetaculosPorIdsAsync(
                predicoes.Select(p => p.ItemId),
                cancellationToken);

            return predicoes
                .Where(p => espetaculos.ContainsKey(p.ItemId))
                .Select(p => new RecomendacaoDto
                {
                    Score = p.Score,
                    TipoAlgoritmo = p.TipoAlgoritmo,
                    Motivo = p.Motivo,
                    DataGeracao = DateTime.UtcNow,
                    Espetaculo = MapearEspetaculo(espetaculos[p.ItemId])
                })
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Falha ao executar modelo incrementado; usando recomendacoes pre-processadas.");
            return [];
        }
    }

    private static int NormalizarLimite(int? limite) => limite switch
    {
        null or < 1 => LimitePadrao,
        > LimiteMaximo => LimiteMaximo,
        _ => limite.Value
    };

    private static RecomendacaoDto Mapear(Domain.Entities.Recomendacao recomendacao) => new()
    {
        Score = recomendacao.Score,
        TipoAlgoritmo = recomendacao.TipoAlgoritmo,
        Motivo = recomendacao.Motivo,
        DataGeracao = recomendacao.DataGeracao,
        Espetaculo = MapearEspetaculo(recomendacao.Espetaculo)
    };

    private static EspetaculoRecomendadoDto MapearEspetaculo(Domain.Entities.EspetaculoRecomendado espetaculo) => new()
    {
        Id = espetaculo.Id,
        Titulo = espetaculo.Titulo,
        Genero = espetaculo.Genero,
        Status = espetaculo.Status,
        NotaMedia = espetaculo.NotaMedia
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
