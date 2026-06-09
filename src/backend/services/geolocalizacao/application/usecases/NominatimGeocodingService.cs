using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Spotlight.Geolocalizacao.Application.Dtos;
using Spotlight.Geolocalizacao.Domain.Entities;
using Spotlight.Geolocalizacao.Infrastructure.Persistence;

namespace Spotlight.Geolocalizacao.Application.UseCases;

public interface IGeocodingService
{
    Task<GeocodingResponse> GeocodeAsync(string address, CancellationToken cancellationToken);

    Task<GeocodingResponse> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

public sealed class NominatimGeocodingService(
    HttpClient httpClient,
    IMemoryCache cache,
    IGeocodeCacheRepository cacheRepository)
    : IGeocodingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

    public async Task<GeocodingResponse> GeocodeAsync(string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["address"] = ["O endereco e obrigatorio."]
            });
        }

        var normalizedAddress = NormalizeAddress(address);
        var cacheKey = $"geocode:{normalizedAddress}";
        if (cache.TryGetValue(cacheKey, out GeocodingResponse? cached) && cached is not null)
        {
            return cached;
        }

        var path = $"/search?q={Uri.EscapeDataString(normalizedAddress)}&format=jsonv2&addressdetails=1&limit=1";
        var payload = await SendAsync<IReadOnlyList<NominatimPlace>>(path, cancellationToken);
        var place = payload.FirstOrDefault()
            ?? throw new NotFoundException("Localizacao nao encontrada.");

        var response = ToResponse(place);
        cache.Set(cacheKey, response, CacheTtl);

        return response;
    }

    public async Task<GeocodingResponse> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["latitude"] = ["Latitude deve estar entre -90 e 90."],
                ["longitude"] = ["Longitude deve estar entre -180 e 180."]
            });
        }

        var roundedLatitude = Math.Round(latitude, 5);
        var roundedLongitude = Math.Round(longitude, 5);
        var cacheKey = FormattableString.Invariant($"reverse:{roundedLatitude}:{roundedLongitude}");
        if (cache.TryGetValue(cacheKey, out GeocodingResponse? cached) && cached is not null)
        {
            return cached;
        }

        var lat = roundedLatitude.ToString(CultureInfo.InvariantCulture);
        var lon = roundedLongitude.ToString(CultureInfo.InvariantCulture);
        var path = $"/reverse?lat={lat}&lon={lon}&format=jsonv2&addressdetails=1";
        var place = await SendAsync<NominatimPlace>(path, cancellationToken);

        if (string.IsNullOrWhiteSpace(place.DisplayName))
        {
            throw new NotFoundException("Localizacao nao encontrada.");
        }

        var response = ToResponse(place);
        cache.Set(cacheKey, response, CacheTtl);
        await cacheRepository.SaveReverseAsync(response, cancellationToken);
        return response;
    }

    private async Task<TPayload> SendAsync<TPayload>(string path, CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(path, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ConflictException("Falha ao consultar OpenStreetMap/Nominatim.");
        }

        return JsonSerializer.Deserialize<TPayload>(body, JsonOptions)
            ?? throw new ConflictException("Resposta vazia do OpenStreetMap/Nominatim.");
    }

    private async Task<HttpResponseMessage> SendRequestAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(path, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ConflictException($"Falha ao consultar OpenStreetMap/Nominatim: {ex.Message}");
        }
    }

    private static GeocodingResponse ToResponse(NominatimPlace place)
    {
        var latitude = double.Parse(place.Lat, CultureInfo.InvariantCulture);
        var longitude = double.Parse(place.Lon, CultureInfo.InvariantCulture);

        return new GeocodingResponse(
            place.DisplayName,
            latitude,
            longitude,
            place.PlaceId.ToString(CultureInfo.InvariantCulture),
            "openstreetmap_nominatim");
    }

    private static string NormalizeAddress(string address) =>
        string.Join(' ', address.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private sealed record NominatimPlace(
        [property: JsonPropertyName("place_id")] long PlaceId,
        [property: JsonPropertyName("display_name")] string DisplayName,
        [property: JsonPropertyName("lat")] string Lat,
        [property: JsonPropertyName("lon")] string Lon);
}
