using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Infrastructure.Http;

public sealed class MotorSimilaridadeHttpClient(HttpClient httpClient) : IMotorSimilaridadeClient
{
    public async Task ValidarConsultaAsync(
        int espetaculoOrigemId,
        int limite,
        CancellationToken cancellationToken)
    {
        var caminho = $"/knn/espetaculos/{espetaculoOrigemId}/similares?limite={limite}";
        using var resposta = await httpClient.GetAsync(caminho, cancellationToken);

        if (!resposta.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Motor de similaridade indisponivel. Status recebido: {(int)resposta.StatusCode}.");
        }
    }
}
