using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Globalization;
using System.Text.Json;

namespace Spotlight.Geolocation.Controllers;

[ApiController]
[Route("geolocalizacao/geocoding")]
public class GeolocationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NpgsqlDataSource? _dataSource;
    private readonly IConfiguration _configuration;

    public GeolocationController(IHttpClientFactory httpClientFactory, IConfiguration configuration, NpgsqlDataSource? dataSource = null)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _dataSource = dataSource;
    }

    [HttpGet("reverse")]
    public async Task<IActionResult> Reverse([FromQuery] decimal lat, [FromQuery] decimal lng)
    {
        var baseUrl = _configuration["Geocoding:BaseUrl"] ?? "https://maps.googleapis.com";
        var client = _httpClientFactory.CreateClient();
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);
        var url = $"{baseUrl.TrimEnd('/')}/maps/api/geocode/json?latlng={latText},{lngText}&key=TEST_KEY";
        var resp = await client.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

        using var stream = await resp.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;
        var results = root.GetProperty("results");
        var formatted = results[0].GetProperty("formatted_address").GetString() ?? string.Empty;

        // Persist in DB if available
        if (_dataSource is not null)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO geocode_cache (lat, lng, address) VALUES (@lat, @lng, @addr);";
            cmd.Parameters.AddWithValue("lat", lat);
            cmd.Parameters.AddWithValue("lng", lng);
            cmd.Parameters.AddWithValue("addr", formatted);
            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { address = formatted, lat, lng });
    }
}
