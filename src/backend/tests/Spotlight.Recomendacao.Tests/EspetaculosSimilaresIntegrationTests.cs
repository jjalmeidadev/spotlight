using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

public sealed class EspetaculosSimilaresIntegrationTests(
    EspetaculosSimilaresIntegrationTests.Fixture fixture)
    : IClassFixture<EspetaculosSimilaresIntegrationTests.Fixture>
{
    [Fact]
    public async Task GetSimilares_QuandoMotorKnnDisponivel_RetornaSimilaresOrdenadosDoPostgres()
    {
        fixture.MotorSimilaridade
            .Given(Request.Create()
                .WithPath("/knn/espetaculos/101/similares")
                .WithParam("limite", "2")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    algoritmo = "k-NN",
                    origem = 101,
                    limite = 2
                }));

        using var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/espetaculos/101/similares?limite=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");

        Assert.Equal(2, data.GetArrayLength());
        Assert.Equal(202, data[0].GetProperty("espetaculo").GetProperty("id").GetInt32());
        Assert.Equal("Mamma Mia!", data[0].GetProperty("espetaculo").GetProperty("titulo").GetString());
        Assert.Equal(0.932100m, data[0].GetProperty("score").GetDecimal());
        Assert.Equal("k-NN", data[0].GetProperty("tipoAlgoritmo").GetString());

        Assert.Equal(303, data[1].GetProperty("espetaculo").GetProperty("id").GetInt32());
        Assert.Equal("Hairspray", data[1].GetProperty("espetaculo").GetProperty("titulo").GetString());

        Assert.Contains(fixture.MotorSimilaridade.LogEntries, entry =>
            entry.RequestMessage.Path == "/knn/espetaculos/101/similares"
            && entry.RequestMessage.Url?.Contains("limite=2", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task GetSimilares_QuandoMotorKnnIndisponivel_RetornaErroInterno()
    {
        fixture.MotorSimilaridade
            .Given(Request.Create()
                .WithPath("/knn/espetaculos/101/similares")
                .WithParam("limite", "3")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(503)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    error = "motor indisponivel"
                }));

        using var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/espetaculos/101/similares?limite=3");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Erro interno no servidor.", json.RootElement.GetProperty("error").GetString());

        Assert.Contains(fixture.MotorSimilaridade.LogEntries, entry =>
            entry.RequestMessage.Path == "/knn/espetaculos/101/similares"
            && entry.RequestMessage.Url?.Contains("limite=3", StringComparison.Ordinal) == true);
    }

    public sealed class Fixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("spotlight_recomendacao_tests")
            .WithUsername("spotlight")
            .WithPassword("spotlight")
            .Build();

        private string? _databaseConnectionStringOriginal;
        private string? _motorSimilaridadeBaseUrlOriginal;

        public WebApplicationFactory<Program> Factory { get; private set; } = null!;
        public WireMockServer MotorSimilaridade { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            await CriarSchemaESeedAsync(_postgres.GetConnectionString());

            MotorSimilaridade = WireMockServer.Start();

            _databaseConnectionStringOriginal =
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            _motorSimilaridadeBaseUrlOriginal =
                Environment.GetEnvironmentVariable("MOTOR_SIMILARIDADE_BASE_URL");

            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _postgres.GetConnectionString());
            Environment.SetEnvironmentVariable("MOTOR_SIMILARIDADE_BASE_URL", MotorSimilaridade.Urls[0]);

            Factory = new WebApplicationFactory<Program>();
        }

        public async Task DisposeAsync()
        {
            Factory.Dispose();
            MotorSimilaridade.Stop();
            MotorSimilaridade.Dispose();
            await _postgres.DisposeAsync();

            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _databaseConnectionStringOriginal);
            Environment.SetEnvironmentVariable("MOTOR_SIMILARIDADE_BASE_URL", _motorSimilaridadeBaseUrlOriginal);
        }

        private static async Task CriarSchemaESeedAsync(string connectionString)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TYPE status_espetaculo_enum AS ENUM (
                    'em_cartaz',
                    'em_breve',
                    'encerrado'
                );

                CREATE TABLE espetaculo (
                    id_espetaculo SERIAL PRIMARY KEY,
                    titulo VARCHAR(200) NOT NULL,
                    descricao TEXT,
                    genero VARCHAR(100),
                    faixa_etaria VARCHAR(50),
                    duracao INTEGER CHECK (duracao > 0),
                    classificacao_indicativa VARCHAR(20),
                    produtora VARCHAR(150),
                    status status_espetaculo_enum NOT NULL DEFAULT 'em_breve',
                    popularidade NUMERIC(10, 4) NOT NULL DEFAULT 0,
                    nota_media NUMERIC(3, 2) NOT NULL DEFAULT 0 CHECK (nota_media BETWEEN 0 AND 5),
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
                );

                CREATE TABLE similaridade_item (
                    id_similaridade SERIAL PRIMARY KEY,
                    id_espetaculo_origem INTEGER NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
                    id_espetaculo_similar INTEGER NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
                    tipo_algoritmo VARCHAR(50) NOT NULL,
                    score_similaridade NUMERIC(8, 6) NOT NULL CHECK (score_similaridade >= 0 AND score_similaridade <= 1),
                    data_geracao TIMESTAMP NOT NULL DEFAULT NOW(),
                    CONSTRAINT chk_tipo_algoritmo_similar CHECK (tipo_algoritmo IN ('k-NN', 'SVD', 'k-means')),
                    CONSTRAINT chk_espetaculos_distintos CHECK (id_espetaculo_origem <> id_espetaculo_similar),
                    CONSTRAINT uq_similaridade_item UNIQUE (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo)
                );

                CREATE INDEX idx_similaridade_item_origem
                    ON similaridade_item(id_espetaculo_origem, score_similaridade DESC);

                INSERT INTO espetaculo
                    (id_espetaculo, titulo, genero, status, popularidade, nota_media)
                VALUES
                    (101, 'A Novica Rebelde', 'Teatro Musical', 'em_cartaz', 0.9100, 4.70),
                    (202, 'Mamma Mia!', 'Teatro Musical', 'em_cartaz', 0.9500, 4.80),
                    (303, 'Hairspray', 'Teatro Musical', 'em_cartaz', 0.8700, 4.60);

                INSERT INTO similaridade_item
                    (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo, score_similaridade)
                VALUES
                    (101, 303, 'k-NN', 0.811200),
                    (101, 202, 'k-NN', 0.932100);
                """;

            await command.ExecuteNonQueryAsync();
        }
    }
}
