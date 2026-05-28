using Microsoft.AspNetCore.Mvc;
using Spotlight.Geolocalizacao.Application.Dtos;
using Spotlight.Geolocalizacao.Application.UseCases;

namespace Spotlight.Geolocalizacao.Presentation.Controllers;

[ApiController]
[Route("api/geocoding")]
public sealed class GeocodingController(IGeocodingService geocodingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Geocode([FromQuery] string address, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await geocodingService.GeocodeAsync(address, cancellationToken));
        }
        catch (GeocodingRequestException ex)
        {
            return StatusCode(ex.StatusCode, Error(ex.Message));
        }
        catch (GeocodingNotFoundException ex)
        {
            return NotFound(Error(ex.Message));
        }
    }

    [HttpGet("reverse")]
    [ProducesResponseType(typeof(GeocodingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReverseGeocode(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken));
        }
        catch (GeocodingRequestException ex)
        {
            return StatusCode(ex.StatusCode, Error(ex.Message));
        }
        catch (GeocodingNotFoundException ex)
        {
            return NotFound(Error(ex.Message));
        }
    }

    private object Error(string message) => new
    {
        error = message,
        traceId = HttpContext.TraceIdentifier
    };
}
