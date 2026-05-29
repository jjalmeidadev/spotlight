using Microsoft.AspNetCore.Mvc;
using Spotlight.Geolocalizacao.Application.Dtos;
using Spotlight.Geolocalizacao.Application.UseCases;

namespace Spotlight.Geolocalizacao.Presentation.Controllers;

/// <summary>
/// Geocodificação via OpenStreetMap/Nominatim. Rotas públicas. O prefixo
/// <c>/geolocalizacao</c> é removido pelo API Gateway.
/// </summary>
[ApiController]
[Route("geocoding")]
public sealed class GeocodingController(IGeocodingService geocodingService) : ControllerBase
{
    /// <summary>Geocodifica um endereço textual em coordenadas (lat/long).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeocodingResponse>> Geocode(
        [FromQuery] string address,
        CancellationToken cancellationToken) =>
        Ok(await geocodingService.GeocodeAsync(address, cancellationToken));

    /// <summary>Geocodificação reversa: coordenadas (lat/long) em endereço estruturado.</summary>
    [HttpGet("reverse")]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeocodingResponse>> ReverseGeocode(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken cancellationToken) =>
        Ok(await geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken));
}
