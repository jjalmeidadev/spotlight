using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Infrastructure.Http;

public sealed class MotorSimilaridadeNoOpClient : IMotorSimilaridadeClient
{
    public Task ValidarConsultaAsync(
        int espetaculoOrigemId,
        int limite,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
