using System.Net.Http.Json;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Infrastructure.Recommendation;

public sealed class HttpPreferenciasRecomendacaoNotifier(HttpClient httpClient)
    : IPreferenciasRecomendacaoNotifier
{
    public async Task NotificarPreferenciasAtualizadasAsync(
        Guid usuarioId,
        Preferencias preferencias,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "/recommendation/preferences-updated",
                new { usuarioId, preferencias },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ConflictException("Motor de recomendacao indisponivel.");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new ConflictException($"Falha ao notificar motor de recomendacao: {ex.Message}");
        }
    }
}

public sealed class NoOpPreferenciasRecomendacaoNotifier : IPreferenciasRecomendacaoNotifier
{
    public Task NotificarPreferenciasAtualizadasAsync(
        Guid usuarioId,
        Preferencias preferencias,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
