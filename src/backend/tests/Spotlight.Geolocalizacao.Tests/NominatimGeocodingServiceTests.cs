using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Spotlight.Geolocalizacao.Application.UseCases;
using Xunit;
using GeoConflictException = Spotlight.Geolocalizacao.Domain.Entities.ConflictException;
using GeoNotFoundException = Spotlight.Geolocalizacao.Domain.Entities.NotFoundException;
using GeoValidationException = Spotlight.Geolocalizacao.Domain.Entities.ValidationException;

namespace Spotlight.Geolocalizacao.Tests;

public class NominatimGeocodingServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task Geocode_ComEnderecoVazio_LancaValidation()
    {
        var service = CreateService(new QueueHandler());

        var ex = await Assert.ThrowsAsync<GeoValidationException>(() => service.GeocodeAsync("   ", Ct));

        Assert.Contains("address", ex.Details.Keys);
    }

    [Fact]
    public async Task Geocode_NormalizaEnderecoEUsaCache()
    {
        var handler = new QueueHandler();
        handler.Enqueue(HttpStatusCode.OK, """
            [
              {
                "place_id": 123,
                "display_name": "Avenida Paulista, Sao Paulo",
                "lat": "-23.56141",
                "lon": "-46.65588"
              }
            ]
            """);
        var service = CreateService(handler);

        var primeiro = await service.GeocodeAsync("  AV Paulista   1000 ", Ct);
        var segundo = await service.GeocodeAsync("av paulista 1000", Ct);

        Assert.Equal("Avenida Paulista, Sao Paulo", primeiro.FormattedAddress);
        Assert.Equal(-23.56141, primeiro.Latitude);
        Assert.Equal("openstreetmap_nominatim", primeiro.Provider);
        Assert.Same(primeiro, segundo);
        Assert.Equal(1, handler.Requests.Count);
        Assert.Contains("/search?q=av%20paulista%201000", handler.Requests.Single());
    }

    [Fact]
    public async Task Geocode_QuandoApiNaoRetornaLugar_LancaNotFound()
    {
        var handler = new QueueHandler();
        handler.Enqueue(HttpStatusCode.OK, "[]");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<GeoNotFoundException>(() => service.GeocodeAsync("Endereco inexistente", Ct));
    }

    [Fact]
    public async Task ReverseGeocode_ComCoordenadasInvalidas_LancaValidation()
    {
        var service = CreateService(new QueueHandler());

        var ex = await Assert.ThrowsAsync<GeoValidationException>(() => service.ReverseGeocodeAsync(-91, 181, Ct));

        Assert.Contains("latitude", ex.Details.Keys);
        Assert.Contains("longitude", ex.Details.Keys);
    }

    [Fact]
    public async Task ReverseGeocode_ArredondaCoordenadasEUsaCache()
    {
        var handler = new QueueHandler();
        handler.Enqueue(HttpStatusCode.OK, """
            {
              "place_id": 456,
              "display_name": "Teatro Alfa, Sao Paulo",
              "lat": "-23.55000",
              "lon": "-46.63000"
            }
            """);
        var service = CreateService(handler);

        var primeiro = await service.ReverseGeocodeAsync(-23.55, -46.63, Ct);
        var segundo = await service.ReverseGeocodeAsync(-23.550004, -46.630004, Ct);

        Assert.Equal("Teatro Alfa, Sao Paulo", primeiro.FormattedAddress);
        Assert.Equal("456", primeiro.PlaceId);
        Assert.Same(primeiro, segundo);
        Assert.Equal(1, handler.Requests.Count);
        Assert.Contains("/reverse?lat=-23.55&lon=-46.63", handler.Requests.Single());
    }

    [Fact]
    public async Task ReverseGeocode_ComDisplayNameVazio_LancaNotFound()
    {
        var handler = new QueueHandler();
        handler.Enqueue(HttpStatusCode.OK, """
            { "place_id": 456, "display_name": "", "lat": "-23.55", "lon": "-46.63" }
            """);
        var service = CreateService(handler);

        await Assert.ThrowsAsync<GeoNotFoundException>(() => service.ReverseGeocodeAsync(-23.55, -46.63, Ct));
    }

    [Fact]
    public async Task Geocode_QuandoApiFalha_LancaConflict()
    {
        var handler = new QueueHandler();
        handler.Enqueue(HttpStatusCode.TooManyRequests, """{ "error": "rate limit" }""");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<GeoConflictException>(() => service.GeocodeAsync("Rua A", Ct));
    }

    private static NominatimGeocodingService CreateService(QueueHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://nominatim.test")
        };
        return new NominatimGeocodingService(client, new MemoryCache(new MemoryCacheOptions()));
    }

    private sealed class QueueHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new();

        public List<string> Requests { get; } = [];

        public void Enqueue(HttpStatusCode statusCode, string content) =>
            responses.Enqueue(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri?.PathAndQuery ?? string.Empty);

            if (responses.Count == 0)
            {
                throw new InvalidOperationException("Nenhuma resposta HTTP fake configurada.");
            }

            return Task.FromResult(responses.Dequeue());
        }
    }
}
