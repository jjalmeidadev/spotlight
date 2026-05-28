using Microsoft.AspNetCore.Mvc;
using Spotlight.Geolocalizacao.Application.Dtos;
using Spotlight.Geolocalizacao.Application.UseCases;

namespace Spotlight.Geolocalizacao.Presentation.Controllers;

[ApiController]
[Route("geocoding")]
public sealed class GeocodingController(IGeocodingService geocodingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GeocodingResponse>> Geocode([FromQuery] string address, CancellationToken cancellationToken) =>
        Ok(await geocodingService.GeocodeAsync(address, cancellationToken));

    [HttpGet("reverse")]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GeocodingResponse>> ReverseGeocode(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken cancellationToken) =>
        Ok(await geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken));
}
