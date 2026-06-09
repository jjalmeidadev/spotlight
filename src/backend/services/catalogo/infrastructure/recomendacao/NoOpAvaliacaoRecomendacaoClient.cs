using Spotlight.Catalogo.Application.UseCases;

namespace Spotlight.Catalogo.Infrastructure.Recomendacao;

public sealed class NoOpAvaliacaoRecomendacaoClient : IAvaliacaoRecomendacaoClient
{
    public Task NotificarAvaliacaoCriadaAsync(
        AvaliacaoCriadaEvento evento,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
