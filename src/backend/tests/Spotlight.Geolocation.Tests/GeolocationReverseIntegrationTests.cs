using System.Net;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
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
        _wireMock.Given(Request.Create().WithPath("/maps/api/geocode/json").UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{ ""results"": [ { ""formatted_address"": ""Rua Falsa, 123, Cidade Exemplo"", ""geometry"": { ""location"": { ""lat"": -22.9, ""lng"": -47.1 } } } ], ""status"": ""OK"" }")
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
        var lat = -22.9m;
        var lng = -47.1m;
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);
        var res = await client.GetAsync($"/geolocalizacao/geocoding/reverse?lat={latText}&lng={lngText}");
        var json = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK, $"Expected 200 OK but got {(int)res.StatusCode}. Body: {json}");

        var doc = JsonDocument.Parse(json);
        var addr = doc.RootElement.GetProperty("address").GetString();
        Assert.Equal("Rua Falsa, 123, Cidade Exemplo", addr);

        // verify WireMock received call
        Assert.True(_wireMock.LogEntries.Any(), "WireMock did not receive any requests");

        // verify persisted in DB
        await using var conn = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM geocode_cache WHERE lat = @lat AND lng = @lng AND address = @addr";
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
        _wireMock.Given(Request.Create().WithPath("/maps/api/geocode/json").UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(503)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{ ""status"": ""UNAVAILABLE"", ""error_message"": ""Geocoding provider unavailable"" }")
            );

        using var client = _factory!.CreateClient();
        var lat = -23.5m;
        var lng = -46.6m;
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);
        var res = await client.GetAsync($"/geolocalizacao/geocoding/reverse?lat={latText}&lng={lngText}");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
        Assert.True(_wireMock.LogEntries.Any(), "WireMock did not receive the failing geocoding request");

        await using var conn = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM geocode_cache WHERE lat = @lat AND lng = @lng";
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
                    ["Geocoding:BaseUrl"] = _geoBaseUrl
                };
                conf.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<NpgsqlDataSource>();
                services.AddSingleton(NpgsqlDataSource.Create(_connStr));
            });
        }
    }
}
