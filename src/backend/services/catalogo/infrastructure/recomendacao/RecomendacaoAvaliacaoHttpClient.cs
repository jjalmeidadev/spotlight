using System.Net.Http.Json;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Recomendacao;

public sealed class RecomendacaoAvaliacaoHttpClient(HttpClient httpClient)
    : IAvaliacaoRecomendacaoClient
{
    private const string Endpoint = "eventos/avaliacoes";

    public async Task NotificarAvaliacaoCriadaAsync(
        AvaliacaoCriadaEvento evento,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(Endpoint, evento, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalDependencyException(
                    $"Servico de recomendacao respondeu {(int)response.StatusCode} ao receber avaliacao.");
            }
        }
        catch (ExternalDependencyException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalDependencyException("Servico de recomendacao excedeu o tempo de resposta.");
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalDependencyException("Servico de recomendacao indisponivel.", ex);
        }
    }
}
