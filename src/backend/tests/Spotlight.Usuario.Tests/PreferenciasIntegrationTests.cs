using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Spotlight.Usuario.Tests;

[CollectionDefinition(nameof(PreferenciasIntegrationCollection), DisableParallelization = true)]
public sealed class PreferenciasIntegrationCollection : ICollectionFixture<PreferenciasIntegrationFixture>;

[Collection(nameof(PreferenciasIntegrationCollection))]
public class PreferenciasIntegrationTests(PreferenciasIntegrationFixture fixture)
{
    private const string MotorPath = "/recommendation/preferences-updated";

    [Fact]
    public async Task DeveAtualizarPreferenciasQuandoMotorRecomendacaoConfirma()
    {
        fixture.ResetWireMock();
        fixture.WireMock
            .Given(Request.Create().WithPath(MotorPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        using var client = fixture.CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync("/me/preferencias", new
        {
            generosFavoritos = new[] { "Teatro Musical", "Dramaturgia" },
            categoriasFavoritas = new[] { 1, 3 },
            faixaPrecoMax = 150
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var leitura = await client.GetAsync("/me/preferencias");
        Assert.Equal(HttpStatusCode.OK, leitura.StatusCode);
        var preferenciasApi = await leitura.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Teatro Musical", preferenciasApi.GetProperty("generosFavoritos")[0].GetString());
        Assert.Equal(150, preferenciasApi.GetProperty("faixaPrecoMax").GetDecimal());

        var preferencias = await fixture.ObterPreferenciasPersistidasAsync();
        Assert.Equal("Teatro Musical", preferencias.GetProperty("generosFavoritos")[0].GetString());
        Assert.Equal(150, preferencias.GetProperty("faixaPrecoMax").GetDecimal());

        var chamadas = fixture.WireMock.FindLogEntries(Request.Create().WithPath(MotorPath).UsingPost());
        Assert.Single(chamadas);
    }

    [Fact]
    public async Task DeveRetornarConflitoQuandoMotorRecomendacaoEstaIndisponivel()
    {
        fixture.ResetWireMock();
        fixture.WireMock
            .Given(Request.Create().WithPath(MotorPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.ServiceUnavailable));

        using var client = fixture.CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync("/me/preferencias", new
        {
            generosFavoritos = new[] { "Teatro Infantil" },
            categoriasFavoritas = new[] { 2 },
            faixaPrecoMax = 80
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Motor de recomendacao indisponivel", error.GetProperty("error").GetString());

        var chamadas = fixture.WireMock.FindLogEntries(Request.Create().WithPath(MotorPath).UsingPost());
        Assert.Single(chamadas);
    }
}

public sealed class PreferenciasIntegrationFixture : IAsyncLifetime
{
    private const string JwtSecret = "spotlight-aula-8-integration-secret";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("spotlight_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly Dictionary<string, string?> _env = new();
    private WebApplicationFactory<Program>? _factory;

    public Guid UsuarioId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public WireMockServer WireMock { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        WireMock = WireMockServer.Start();

        SetEnv("DATABASE_CONNECTION_STRING", _postgres.GetConnectionString());
        SetEnv("RECOMMENDATION_ENGINE_BASE_URL", WireMock.Urls[0]);
        SetEnv("SUPABASE_JWT_SECRET", JwtSecret);
        SetEnv("SUPABASE_AUTH_JWKS_URL", null);
        SetEnv("SUPABASE_URL", null);
        SetEnv("EXPO_PUBLIC_SUPABASE_URL", null);

        await AplicarSchemaAsync();
        await InserirUsuarioAsync();

        _factory = new WebApplicationFactory<Program>();
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        WireMock.Stop();
        WireMock.Dispose();
        await _postgres.DisposeAsync();

        foreach (var item in _env)
        {
            Environment.SetEnvironmentVariable(item.Key, item.Value);
        }
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = (_factory ?? throw new InvalidOperationException("Factory nao inicializada.")).CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CriarJwt());
        return client;
    }

    public void ResetWireMock()
    {
        WireMock.ResetMappings();
        WireMock.ResetLogEntries();
    }

    public async Task<JsonElement> ObterPreferenciasPersistidasAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT perfil_preferencias::text FROM usuario WHERE id_usuario = @id;";
        command.Parameters.AddWithValue("id", UsuarioId);

        var raw = (string?)await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("Usuario de teste nao encontrado.");

        using var document = JsonDocument.Parse(raw);
        return document.RootElement.Clone();
    }

    private async Task AplicarSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        var databasePath = FindDatabasePath();

        foreach (var path in new[]
        {
            Path.Combine(databasePath, "00_supabase_compat.sql"),
            Path.Combine(databasePath, "01_create_database.sql")
        })
        {
            await using var command = connection.CreateCommand();
            command.CommandText = await File.ReadAllTextAsync(path);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static string FindDatabasePath([CallerFilePath] string sourcePath = "")
    {
        var sourceDirectory = Path.GetDirectoryName(sourcePath);
        if (!string.IsNullOrWhiteSpace(sourceDirectory))
        {
            var databasePathFromSource = Path.GetFullPath(
                Path.Combine(sourceDirectory, "..", "..", "..", "database"));

            if (Directory.Exists(databasePathFromSource))
            {
                return databasePathFromSource;
            }
        }

        foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startPath);

            while (directory is not null)
            {
                var databasePath = Path.Combine(directory.FullName, "src", "database");

                if (Directory.Exists(databasePath))
                {
                    return databasePath;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Diretorio src/database nao encontrado a partir do diretorio de testes.");
    }

    private async Task InserirUsuarioAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO auth.users (id, aud, role, email, raw_user_meta_data, email_confirmed_at)
            VALUES (@id, 'authenticated', 'authenticated', 'mari.integration@example.com',
                    '{"nome": "Mari Integracao"}'::jsonb, NOW());
            """;
        command.Parameters.AddWithValue("id", UsuarioId);
        await command.ExecuteNonQueryAsync();
    }

    private void SetEnv(string key, string? value)
    {
        if (!_env.ContainsKey(key))
        {
            _env[key] = Environment.GetEnvironmentVariable(key);
        }

        Environment.SetEnvironmentVariable(key, value);
    }

    private string CriarJwt()
    {
        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new
        {
            sub = UsuarioId.ToString(),
            tipo_usuario = "cadastrado",
            exp = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(JwtSecret));
        var signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
        return $"{unsignedToken}.{signature}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
