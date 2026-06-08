using System.Globalization;
using System.Text.Json;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.ValueObjects;

namespace Spotlight.Recomendacao.Infrastructure.Geocoding;

/// <summary>
/// Implementação de <see cref="IGeocodingClient"/> sobre a API OpenStreetMap/Nominatim.
/// O <see cref="HttpClient"/> é configurado por DI (base URL, timeout e User-Agent)
/// em <c>Program.cs</c> a partir de <c>GEOCODING_BASE_URL</c>.
///
/// Faz <c>GET /reverse?lat={lat}&amp;lon={lon}&amp;format=jsonv2&amp;addressdetails=1</c> e
/// extrai um rótulo de cidade/região. Em status não-2xx lança via
/// <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>; a degradação graciosa
/// (RNF03) é responsabilidade do caso de uso, que captura qualquer falha.
/// </summary>
public sealed class NominatimGeocodingClient(HttpClient httpClient) : IGeocodingClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string?> ReverseGeocodeAsync(Coordenadas coordenadas, CancellationToken cancellationToken)
    {
        var lat = coordenadas.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = coordenadas.Longitude.ToString(CultureInfo.InvariantCulture);
        var path = $"/reverse?lat={lat}&lon={lon}&format=jsonv2&addressdetails=1";

        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtrairRotulo(body);
    }

    /// <summary>
    /// Prefere o componente mais específico de <c>address</c> (cidade/município);
    /// na ausência, cai para o <c>display_name</c>. Retorna <c>null</c> se nada útil.
    /// </summary>
    private static string? ExtrairRotulo(string body)
    {
        var place = JsonSerializer.Deserialize<NominatimReverse>(body, JsonOptions);
        if (place is null)
        {
            return null;
        }

        var address = place.Address;
        var rotulo = address?.City
            ?? address?.Town
            ?? address?.Municipality
            ?? address?.Village
            ?? address?.State
            ?? place.DisplayName;

        return string.IsNullOrWhiteSpace(rotulo) ? null : rotulo;
    }

    private sealed record NominatimReverse(string? DisplayName, NominatimAddress? Address);

    private sealed record NominatimAddress(
        string? City,
        string? Town,
        string? Municipality,
        string? Village,
        string? State);
}
