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
/// Centraliza a regra de negocio de recomendacao personalizada. Nesta etapa, o
/// service consome recomendacoes ja processadas no PostgreSQL; a integracao com
/// MongoDB/interacoes e motor SVD deve ser plugada aqui sem alterar a rota.
/// </summary>
public class RecomendacaoService(IRecomendacaoRepository repositorio) : IRecomendacaoService
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken)
    {
        var limiteNormalizado = NormalizarLimite(limite);
        var itens = await repositorio.ListarPorUsuarioAsync(usuarioId, limiteNormalizado, cancellationToken);

        return new ListaDto<RecomendacaoDto>
        {
            Data = itens.Select(Mapear).ToList()
        };
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
        Espetaculo = new EspetaculoRecomendadoDto
        {
            Id = recomendacao.Espetaculo.Id,
            Titulo = recomendacao.Espetaculo.Titulo,
            Genero = recomendacao.Espetaculo.Genero,
            Status = recomendacao.Espetaculo.Status,
            NotaMedia = recomendacao.Espetaculo.NotaMedia
        }
    };
}
