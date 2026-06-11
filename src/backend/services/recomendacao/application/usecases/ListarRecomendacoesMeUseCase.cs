using Spotlight.Recomendacao.Application.Dtos;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Recomendações pré-processadas do usuário autenticado. Normaliza o
/// <c>limite</c> dentro de 1..50 (default 10) — limites definidos no contrato.
/// </summary>
public class ListarRecomendacoesMeUseCase(IRecomendacaoRepository repositorio)
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<RecomendacaoDto>> ExecutarAsync(
        Guid usuarioId, int? limite, CancellationToken cancellationToken)
    {
        var limiteNorm = limite switch
        {
            null or < 1 => LimitePadrao,
            > LimiteMaximo => LimiteMaximo,
            _ => limite.Value
        };

        var itens = await repositorio.ListarPorUsuarioAsync(usuarioId, limiteNorm, cancellationToken);

        return new ListaDto<RecomendacaoDto>
        {
            Data = itens.Select(r => new RecomendacaoDto
            {
                Score = r.Score,
                TipoAlgoritmo = r.TipoAlgoritmo,
                Motivo = r.Motivo,
                DataGeracao = r.DataGeracao,
                Espetaculo = new EspetaculoRecomendadoDto
                {
                    Id = r.Espetaculo.Id,
                    Titulo = r.Espetaculo.Titulo,
                    Genero = r.Espetaculo.Genero,
                    Status = r.Espetaculo.Status,
                    NotaMedia = r.Espetaculo.NotaMedia,
                    Midias = r.Espetaculo.Midias.Select(ListarSimilaresUseCase.MapearMidia).ToList()
                }
            }).ToList()
        };
    }
}
