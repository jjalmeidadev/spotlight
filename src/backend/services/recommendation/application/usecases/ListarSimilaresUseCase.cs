using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Espetáculos similares a um dado espetáculo (item-item). Lê da tabela
/// <c>similaridade_item</c> — similaridade global, leitura pública.
/// Normaliza o <c>limite</c> dentro de 1..50 (default 10).
/// Lança <see cref="NotFoundException"/> se o espetáculo de origem não existir.
/// </summary>
public class ListarSimilaresUseCase(ISimilaridadeRepository repositorio)
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<SimilarDto>> ExecutarAsync(
        int espetaculoOrigemId, int? limite, CancellationToken cancellationToken)
    {
        if (!await repositorio.EspetaculoExisteAsync(espetaculoOrigemId, cancellationToken))
        {
            throw new NotFoundException($"Espetáculo {espetaculoOrigemId} não encontrado.");
        }

        var limiteNorm = limite switch
        {
            null or < 1 => LimitePadrao,
            > LimiteMaximo => LimiteMaximo,
            _ => limite.Value
        };

        var itens = await repositorio.ListarPorOrigemAsync(
            espetaculoOrigemId, limiteNorm, cancellationToken);

        return new ListaDto<SimilarDto>
        {
            Data = itens.Select(s => new SimilarDto
            {
                Score = s.Score,
                TipoAlgoritmo = s.TipoAlgoritmo,
                Espetaculo = new EspetaculoRecomendadoDto
                {
                    Id = s.Espetaculo.Id,
                    Titulo = s.Espetaculo.Titulo,
                    Genero = s.Espetaculo.Genero,
                    Status = s.Espetaculo.Status,
                    NotaMedia = s.Espetaculo.NotaMedia,
                    Midias = s.Espetaculo.Midias.Select(MapearMidia).ToList()
                }
            }).ToList()
        };
    }

    internal static EspetaculoMidiaDto MapearMidia(EspetaculoMidia m) => new()
    {
        TipoMidia = m.TipoMidia,
        StorageBucket = m.StorageBucket,
        StoragePath = m.StoragePath,
        AltText = m.AltText,
        OrdemExibicao = m.OrdemExibicao
    };
}
