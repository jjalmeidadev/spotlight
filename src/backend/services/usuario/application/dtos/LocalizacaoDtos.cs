namespace Spotlight.Usuario.Application.Dtos;

public sealed record AtualizarLocalizacaoRequest(
    string FormattedAddress,
    double Latitude,
    double Longitude,
    string PlaceId,
    string Provider);
