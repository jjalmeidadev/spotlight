using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Infrastructure.Geolocalizacao;

public sealed class HttpGeocodingClient(HttpClient httpClient) : IGeocodingClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<GeocodingResult> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var path = "geocoding/reverse" +
            $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(path, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ConflictException(
                $"Falha ao consultar servico de geolocalizacao: {ex.Message}");
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    "Coordenadas nao encontradas no servico de geolocalizacao.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ConflictException(
                    $"Servico de geolocalizacao indisponivel (status {(int)response.StatusCode}).");
            }

            var payload = await response.Content.ReadFromJsonAsync<GeocodingResult>(
                JsonOptions, cancellationToken);

            return payload
                ?? throw new ConflictException(
                    "Resposta vazia do servico de geolocalizacao.");
        }
    }
}
