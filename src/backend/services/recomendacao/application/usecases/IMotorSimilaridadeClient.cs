namespace Spotlight.Recomendacao.Application.UseCases;

public interface IMotorSimilaridadeClient
{
    Task ValidarConsultaAsync(int espetaculoOrigemId, int limite, CancellationToken cancellationToken);
}
