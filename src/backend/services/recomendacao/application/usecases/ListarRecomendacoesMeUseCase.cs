using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.Services;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Recomendações pré-processadas do usuário autenticado. Normaliza o
/// <c>limite</c> dentro de 1..50 (default 10) — limites definidos no contrato.
/// </summary>
public class ListarRecomendacoesMeUseCase(IRecomendacaoService recomendacaoService)
{
    public Task<ListaDto<RecomendacaoDto>> ExecutarAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken) =>
        recomendacaoService.ListarPersonalizadasAsync(usuarioId, limite, cancellationToken);
}
