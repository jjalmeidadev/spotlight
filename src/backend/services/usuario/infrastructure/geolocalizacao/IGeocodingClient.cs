namespace Spotlight.Usuario.Infrastructure.Geolocalizacao;

public sealed record GeocodingResult(
    string FormattedAddress,
    double Latitude,
    double Longitude,
    string PlaceId,
    string Provider);

public interface IGeocodingClient
{
    Task<GeocodingResult> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken);
}
