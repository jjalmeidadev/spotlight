using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.Services;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Caso de uso da rota de recomendacoes do usuario autenticado. A regra de
/// recomendacao fica centralizada em <see cref="IRecomendacaoService" />.
/// </summary>
public class ListarRecomendacoesMeUseCase(IRecomendacaoService recomendacaoService)
{
    public Task<ListaDto<RecomendacaoDto>> ExecutarAsync(
        Guid usuarioId,
        int? limite,
        CancellationToken cancellationToken) =>
        recomendacaoService.ListarPersonalizadasAsync(usuarioId, limite, cancellationToken);
}
