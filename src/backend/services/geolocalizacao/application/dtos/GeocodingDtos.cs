namespace Spotlight.Geolocalizacao.Application.Dtos;

public sealed record CoordinatesResponse(double Latitude, double Longitude);

public sealed record GeocodingResponse(
    string FormattedAddress,
    double Latitude,
    double Longitude,
    string PlaceId,
    string Provider);
