using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spotlight.Geolocalizacao.Application.Dtos;

namespace Spotlight.Geolocalizacao.Application.UseCases;

public interface IGeocodingService
{
    Task<GeocodingResponse> GeocodeAsync(string address, CancellationToken cancellationToken);

    Task<GeocodingResponse> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

public sealed class NominatimGeocodingService(HttpClient httpClient)
    : IGeocodingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GeocodingResponse> GeocodeAsync(string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new GeocodingRequestException("O endereco e obrigatorio.", StatusCodes.Status400BadRequest);
        }

        var path = $"/search?q={Uri.EscapeDataString(address.Trim())}&format=jsonv2&addressdetails=1&limit=1";
        var payload = await SendAsync<IReadOnlyList<NominatimPlace>>(path, cancellationToken);
        var place = payload.FirstOrDefault()
            ?? throw new GeocodingNotFoundException("Localizacao nao encontrada.");

        return ToResponse(place);
    }

    public async Task<GeocodingResponse> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            throw new GeocodingRequestException("Coordenadas invalidas.", StatusCodes.Status400BadRequest);
        }

        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var path = $"/reverse?lat={lat}&lon={lon}&format=jsonv2&addressdetails=1";
        var place = await SendAsync<NominatimPlace>(path, cancellationToken);

        if (string.IsNullOrWhiteSpace(place.DisplayName))
        {
            throw new GeocodingNotFoundException("Localizacao nao encontrada.");
        }

        return ToResponse(place);
    }

    private async Task<TPayload> SendAsync<TPayload>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new GeocodingRequestException("Falha ao consultar OpenStreetMap/Nominatim.", (int)response.StatusCode);
        }

        return JsonSerializer.Deserialize<TPayload>(body, JsonOptions)
            ?? throw new GeocodingRequestException("Resposta vazia do OpenStreetMap/Nominatim.", StatusCodes.Status502BadGateway);
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

    private sealed record NominatimPlace(
        [property: JsonPropertyName("place_id")] long PlaceId,
        [property: JsonPropertyName("display_name")] string DisplayName,
        [property: JsonPropertyName("lat")] string Lat,
        [property: JsonPropertyName("lon")] string Lon);
}

public sealed class GeocodingRequestException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class GeocodingNotFoundException(string message) : Exception(message);
