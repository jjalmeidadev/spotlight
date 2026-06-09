using System.Net;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Spotlight.Geolocalizacao.Application.UseCases;
using Spotlight.Geolocalizacao.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Spotlight.Geolocation.Tests;

public class GeolocationReverseIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private WireMockServer _wireMock = null!;
    private CustomWebApplicationFactory? _factory;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        // create table
        await using (var conn = new NpgsqlConnection(_postgresContainer.GetConnectionString()))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS geocode_cache (
                id SERIAL PRIMARY KEY,
                lat NUMERIC,
                lng NUMERIC,
                address TEXT,
                created_at TIMESTAMPTZ DEFAULT now()
            );";
            await cmd.ExecuteNonQueryAsync();
        }

        _wireMock = WireMockServer.Start();
        _wireMock.Given(Request.Create().WithPath("/reverse").UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{ ""place_id"": 123, ""display_name"": ""Rua Falsa, 123, Cidade Exemplo"", ""lat"": ""-22.9"", ""lon"": ""-47.1"" }")
            );

        _factory = new CustomWebApplicationFactory(_postgresContainer.GetConnectionString(), _wireMock.Urls[0]);
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null) _factory.Dispose();
        if (_wireMock is not null) _wireMock.Stop();
        if (_postgresContainer is not null) await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task ReverseEndpoint_ShouldReturnAddress_AndPersistCache_AndCallExternal()
    {
        using var client = _factory!.CreateClient();
        var lat = -22.9d;
        var lng = -47.1d;
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);
        var res = await client.GetAsync($"/geocoding/reverse?latitude={latText}&longitude={lngText}");
        var json = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK, $"Expected 200 OK but got {(int)res.StatusCode}. Body: {json}");

        var doc = JsonDocument.Parse(json);
        var addr = doc.RootElement.GetProperty("formattedAddress").GetString();
        Assert.Equal("Rua Falsa, 123, Cidade Exemplo", addr);

        // verify WireMock received call
        Assert.True(_wireMock.LogEntries.Any(), "WireMock did not receive any requests");

        // verify persisted in DB
        await using var conn = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM geocode_cache
            WHERE ABS(lat::double precision - @lat) < 0.00001
              AND ABS(lng::double precision - @lng) < 0.00001
              AND address = @addr
            """;
        cmd.Parameters.AddWithValue("lat", lat);
        cmd.Parameters.AddWithValue("lng", lng);
        cmd.Parameters.AddWithValue("addr", addr ?? string.Empty);
        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ReverseEndpoint_ShouldReturnExternalError_AndNotPersistCache_WhenGeocodingFails()
    {
        _wireMock.ResetMappings();
        _wireMock.Given(Request.Create().WithPath("/reverse").UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(503)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{ ""error"": ""Geocoding provider unavailable"" }")
            );

        using var client = _factory!.CreateClient();
        var lat = -23.5d;
        var lng = -46.6d;
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);
        var res = await client.GetAsync($"/geocoding/reverse?latitude={latText}&longitude={lngText}");

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.True(_wireMock.LogEntries.Any(), "WireMock did not receive the failing geocoding request");

        await using var conn = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM geocode_cache
            WHERE ABS(lat::double precision - @lat) < 0.00001
              AND ABS(lng::double precision - @lng) < 0.00001
            """;
        cmd.Parameters.AddWithValue("lat", lat);
        cmd.Parameters.AddWithValue("lng", lng);
        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
        Assert.Equal(0, count);
    }

    private class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connStr;
        private readonly string _geoBaseUrl;

        public CustomWebApplicationFactory(string connStr, string geoBaseUrl)
        {
            _connStr = connStr;
            _geoBaseUrl = geoBaseUrl;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = _connStr,
                    ["Nominatim:BaseUrl"] = _geoBaseUrl,
                    ["Nominatim:UserAgent"] = "spotlight-geolocalizacao-tests/1.0"
                };
                conf.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<NpgsqlDataSource>();
                services.RemoveAll<IGeocodingService>();
                services.RemoveAll<IGeocodeCacheRepository>();
                services.AddSingleton(NpgsqlDataSource.Create(_connStr));
                services.AddScoped<IGeocodeCacheRepository, GeocodeCacheRepository>();
                services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
                {
                    client.BaseAddress = new Uri(_geoBaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("spotlight-geolocalizacao-tests/1.0");
                });
            });
        }
    }
}
