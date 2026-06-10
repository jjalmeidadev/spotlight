using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Spotlight.Usuario.Application.Dtos;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Spotlight.Usuario.Tests;

public sealed class UsuarioLocalizacaoApiFixture : IAsyncLifetime
{
    public const string JwtSecret = "REDACTED_FOR_PUBLIC_RELEASE";

    private const string SchemaSql = """
        CREATE EXTENSION IF NOT EXISTS pgcrypto;

        CREATE SCHEMA IF NOT EXISTS auth;
        CREATE TABLE IF NOT EXISTS auth.users (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid()
        );

        CREATE TYPE tipo_usuario_enum AS ENUM ('visitante', 'cadastrado', 'administrador');

        CREATE TABLE public.usuario (
            id_usuario          UUID                PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
            nome                VARCHAR(150)        NOT NULL,
            email               VARCHAR(255)        NOT NULL UNIQUE,
            data_nascimento     DATE,
            localizacao         JSONB,
            data_cadastro       TIMESTAMP           NOT NULL DEFAULT NOW(),
            tipo_usuario        tipo_usuario_enum   NOT NULL DEFAULT 'cadastrado',
            nivel_acesso        JSONB               NOT NULL DEFAULT '{}'::JSONB,
            perfil_preferencias JSONB               NOT NULL DEFAULT '{}'::JSONB
        );
        """;

    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("spotlight_usuario_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private WireMockServer? geolocalizacao;
    private WebApplicationFactory<Program>? factory;

    public WireMockServer Geolocalizacao => geolocalizacao
        ?? throw new InvalidOperationException("WireMock nao iniciado.");

    public WebApplicationFactory<Program> Factory => factory
        ?? throw new InvalidOperationException("WebApplicationFactory nao iniciada.");

    public string ConnectionString => postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        await ApplySchemaAsync();

        geolocalizacao = WireMockServer.Start();

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", ConnectionString);
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", JwtSecret);
        Environment.SetEnvironmentVariable("SUPABASE_AUTH_JWKS_URL", string.Empty);
        Environment.SetEnvironmentVariable("SUPABASE_URL", string.Empty);
        Environment.SetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_URL", string.Empty);
        Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", string.Empty);
        Environment.SetEnvironmentVariable("MONGODB_DATABASE", string.Empty);
        Environment.SetEnvironmentVariable("MONGODB_TELEMETRY_COLLECTION", string.Empty);
        Environment.SetEnvironmentVariable("GEOLOCALIZACAO_BASE_URL", geolocalizacao.Url!);

        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Test"));

        _ = factory.Services;
    }

    public async Task DisposeAsync()
    {
        if (factory is not null) await factory.DisposeAsync();
        geolocalizacao?.Stop();
        geolocalizacao?.Dispose();
        await postgres.DisposeAsync();
    }

    public async Task<Guid> InsertUsuarioCadastradoAsync(string email)
    {
        var id = Guid.NewGuid();
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using (var insertAuth = new NpgsqlCommand(
            "INSERT INTO auth.users (id) VALUES (@id)", connection))
        {
            insertAuth.Parameters.AddWithValue("id", id);
            await insertAuth.ExecuteNonQueryAsync();
        }

        await using (var insertUsuario = new NpgsqlCommand("""
            INSERT INTO public.usuario (id_usuario, nome, email, tipo_usuario)
            VALUES (@id, @nome, @email, 'cadastrado')
            """, connection))
        {
            insertUsuario.Parameters.AddWithValue("id", id);
            insertUsuario.Parameters.AddWithValue("nome", "Usuario Teste");
            insertUsuario.Parameters.AddWithValue("email", email);
            await insertUsuario.ExecuteNonQueryAsync();
        }

        return id;
    }

    public async Task<string?> GetLocalizacaoAsync(Guid usuarioId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT localizacao::text FROM public.usuario WHERE id_usuario = @id", connection);
        command.Parameters.AddWithValue("id", usuarioId);
        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? null : (string?)result;
    }

    public string BuildJwtFor(Guid usuarioId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim("tipo_usuario", "cadastrado")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task ApplySchemaAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(SchemaSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

[Trait("Category", "Integration")]
public sealed class LocalizacaoIntegrationTests(UsuarioLocalizacaoApiFixture fixture)
    : IClassFixture<UsuarioLocalizacaoApiFixture>
{
    [Fact]
    public async Task PutLocalizacao_QuandoGeoRespondeOk_PersisteCanonizadoNoPostgres()
    {
        const double latitude = -23.55052;
        const double longitude = -46.633308;

        var usuarioId = await fixture.InsertUsuarioCadastradoAsync(
            $"sucesso-{Guid.NewGuid():N}@spotlight.test");

        fixture.Geolocalizacao
            .Given(Request.Create()
                .WithPath("/geocoding/reverse")
                .WithParam("latitude", "-23.55052")
                .WithParam("longitude", "-46.633308")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                      "formattedAddress": "Avenida Paulista, Bela Vista, Sao Paulo, SP, Brasil",
                      "latitude": -23.55052,
                      "longitude": -46.633308,
                      "placeId": "12345",
                      "provider": "openstreetmap_nominatim"
                    }
                    """));

        var payload = new AtualizarLocalizacaoRequest(
            FormattedAddress: "endereco-bagunçado-do-cliente",
            Latitude: latitude,
            Longitude: longitude,
            PlaceId: "n/a",
            Provider: "client_provider");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioId));

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var localizacaoJson = await fixture.GetLocalizacaoAsync(usuarioId);
        Assert.NotNull(localizacaoJson);

        using var saved = JsonDocument.Parse(localizacaoJson!);
        var root = saved.RootElement;

        Assert.Equal("Avenida Paulista, Bela Vista, Sao Paulo, SP, Brasil",
            root.GetProperty("FormattedAddress").GetString());
        Assert.Equal(latitude, root.GetProperty("Latitude").GetDouble());
        Assert.Equal(longitude, root.GetProperty("Longitude").GetDouble());
        Assert.Equal("12345", root.GetProperty("PlaceId").GetString());
        Assert.Equal("openstreetmap_nominatim", root.GetProperty("Provider").GetString());
    }

    [Fact]
    public async Task PutLocalizacao_QuandoGeoIndisponivel_RetornaConflictSemPersistir()
    {
        const double latitude = -22.9068;
        const double longitude = -43.1729;

        var usuarioId = await fixture.InsertUsuarioCadastradoAsync(
            $"falha-{Guid.NewGuid():N}@spotlight.test");

        fixture.Geolocalizacao
            .Given(Request.Create()
                .WithPath("/geocoding/reverse")
                .WithParam("latitude", "-22.9068")
                .WithParam("longitude", "-43.1729")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.ServiceUnavailable)
                .WithBody("Geolocalizacao indisponivel"));

        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioId));

        var payload = new AtualizarLocalizacaoRequest(
            FormattedAddress: "Praia de Copacabana, Rio de Janeiro, RJ, Brasil",
            Latitude: latitude,
            Longitude: longitude,
            PlaceId: "67890",
            Provider: "openstreetmap_nominatim");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioId));

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioId));
    }

    [Fact]
    public async Task PutLocalizacao_ComCoordenadasForaDeRange_RetornaBadRequestSemChamarGeo()
    {
        var usuarioId = await fixture.InsertUsuarioCadastradoAsync(
            $"coords-{Guid.NewGuid():N}@spotlight.test");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioId));

        var payloadInvalido = new AtualizarLocalizacaoRequest(
            FormattedAddress: "Endereco qualquer",
            Latitude: -91.0,
            Longitude: 200.0,
            PlaceId: "n/a",
            Provider: "client");

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payloadInvalido);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioId));

        var chamadasComCoordsInvalidas = fixture.Geolocalizacao.LogEntries
            .Count(entry =>
                entry.RequestMessage.Url.Contains("latitude=-91", StringComparison.Ordinal) ||
                entry.RequestMessage.Url.Contains("longitude=200", StringComparison.Ordinal));
        Assert.Equal(0, chamadasComCoordsInvalidas);
    }

    [Fact]
    public async Task PutLocalizacao_QuandoGeoRetorna404_RetornaNotFoundSemPersistir()
    {
        const double latitude = -19.9167;
        const double longitude = -43.9345;

        var usuarioId = await fixture.InsertUsuarioCadastradoAsync(
            $"geo-404-{Guid.NewGuid():N}@spotlight.test");

        fixture.Geolocalizacao
            .Given(Request.Create()
                .WithPath("/geocoding/reverse")
                .WithParam("latitude", "-19.9167")
                .WithParam("longitude", "-43.9345")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBody("Coordenadas nao encontradas"));

        var payload = new AtualizarLocalizacaoRequest(
            FormattedAddress: "Praca Sete, Belo Horizonte, MG, Brasil",
            Latitude: latitude,
            Longitude: longitude,
            PlaceId: "n/a",
            Provider: "openstreetmap_nominatim");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioId));

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioId));
    }

    [Fact]
    public async Task PutLocalizacao_QuandoGeoRetornaBodyVazio_RetornaConflictSemPersistir()
    {
        const double latitude = -3.7327;
        const double longitude = -38.5267;

        var usuarioId = await fixture.InsertUsuarioCadastradoAsync(
            $"geo-vazio-{Guid.NewGuid():N}@spotlight.test");

        fixture.Geolocalizacao
            .Given(Request.Create()
                .WithPath("/geocoding/reverse")
                .WithParam("latitude", "-3.7327")
                .WithParam("longitude", "-38.5267")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("null"));

        var payload = new AtualizarLocalizacaoRequest(
            FormattedAddress: "Beira Mar, Fortaleza, CE, Brasil",
            Latitude: latitude,
            Longitude: longitude,
            PlaceId: "n/a",
            Provider: "openstreetmap_nominatim");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioId));

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioId));
    }

    [Fact]
    public async Task PutLocalizacao_ParaUsuarioInexistente_RetornaNotFound()
    {
        const double latitude = -15.7942;
        const double longitude = -47.8822;

        var usuarioIdInexistente = Guid.NewGuid();

        fixture.Geolocalizacao
            .Given(Request.Create()
                .WithPath("/geocoding/reverse")
                .WithParam("latitude", "-15.7942")
                .WithParam("longitude", "-47.8822")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                      "formattedAddress": "Praca dos Tres Poderes, Brasilia, DF, Brasil",
                      "latitude": -15.7942,
                      "longitude": -47.8822,
                      "placeId": "99999",
                      "provider": "openstreetmap_nominatim"
                    }
                    """));

        var payload = new AtualizarLocalizacaoRequest(
            FormattedAddress: "Praca dos Tres Poderes, Brasilia, DF, Brasil",
            Latitude: latitude,
            Longitude: longitude,
            PlaceId: "99999",
            Provider: "openstreetmap_nominatim");

        using var apiClient = fixture.Factory.CreateClient();
        apiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.BuildJwtFor(usuarioIdInexistente));

        using var response = await apiClient.PutAsJsonAsync("/me/localizacao", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(await fixture.GetLocalizacaoAsync(usuarioIdInexistente));
    }
}
