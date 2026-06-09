using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Spotlight.Catalogo.Tests;

[Trait("Category", "Integration")]
public sealed class AvaliacaoIntegrationTests : IAsyncLifetime
{
    private const string JwtSecret = "spotlight-integration-tests-secret-with-at-least-32-bytes";
    private static readonly Guid UsuarioId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("spotlight_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WireMockServer? _wireMock;
    private WebApplicationFactory<Program>? _factory;
    private string? _databaseConnectionStringAnterior;
    private string? _jwtSecretAnterior;
    private string? _recomendacaoBaseUrlAnterior;

    public async Task InitializeAsync()
    {
        _databaseConnectionStringAnterior = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        _jwtSecretAnterior = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");
        _recomendacaoBaseUrlAnterior = Environment.GetEnvironmentVariable("RECOMENDACAO_BASE_URL");

        await _postgres.StartAsync();
        _wireMock = WireMockServer.Start();

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", JwtSecret);
        Environment.SetEnvironmentVariable("RECOMENDACAO_BASE_URL", _wireMock.Url);

        await PrepararBancoAsync();

        _factory = new WebApplicationFactory<Program>();
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        _wireMock?.Stop();
        _wireMock?.Dispose();
        await _postgres.DisposeAsync();

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _databaseConnectionStringAnterior);
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", _jwtSecretAnterior);
        Environment.SetEnvironmentVariable("RECOMENDACAO_BASE_URL", _recomendacaoBaseUrlAnterior);
    }

    [Fact]
    public async Task PostCatalogoEspetaculoAvaliacoes_ComDadosValidos_PersisteNoPostgresENotificaRecomendacao()
    {
        _wireMock!
            .Given(Request.Create().WithPath("/eventos/avaliacoes").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Accepted));

        using var client = CriarClienteAutenticado();

        var response = await client.PostAsJsonAsync("/catalogo/espetaculos/1/avaliacoes", new
        {
            nota = 4.5m,
            comentario = "Excelente producao."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AvaliacaoCriadaResponse>();
        Assert.NotNull(body);
        Assert.Equal(4.5m, body!.Nota);
        Assert.Equal("Excelente producao.", body.Comentario);

        await using var conexao = new NpgsqlConnection(_postgres.GetConnectionString());
        await conexao.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM avaliacao WHERE id_usuario = @usuario AND id_espetaculo = 1 AND nota = 4.5;",
            conexao);
        cmd.Parameters.AddWithValue("usuario", UsuarioId);
        Assert.Equal(1L, (long)(await cmd.ExecuteScalarAsync())!);

        var chamada = Assert.Single(_wireMock!.LogEntries);
        Assert.Contains("\"espetaculoId\":1", chamada.RequestMessage.Body);
        Assert.Contains($"\"usuarioId\":\"{UsuarioId}\"", chamada.RequestMessage.Body);
    }

    [Fact]
    public async Task PostCatalogoEspetaculoAvaliacoes_ComNotaForaDaRegra_RetornaBadRequestSemNotificarRecomendacao()
    {
        _wireMock!
            .Given(Request.Create().WithPath("/eventos/avaliacoes").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Accepted));

        using var client = CriarClienteAutenticado(Guid.Parse("10000000-0000-0000-0000-000000000002"));

        var response = await client.PostAsJsonAsync("/catalogo/espetaculos/1/avaliacoes", new
        {
            nota = 0,
            comentario = "Nota fora da regra documentada."
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErroResponse>();
        Assert.Contains("Dados de entrada", payload!.Error);
        Assert.Empty(_wireMock!.LogEntries);
    }

    [Fact]
    public async Task PostCatalogoEspetaculoAvaliacoes_QuandoRecomendacaoFalha_RetornaBadGateway()
    {
        _wireMock!
            .Given(Request.Create().WithPath("/eventos/avaliacoes").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.ServiceUnavailable));

        using var client = CriarClienteAutenticado(Guid.Parse("10000000-0000-0000-0000-000000000002"));

        var response = await client.PostAsJsonAsync("/catalogo/espetaculos/1/avaliacoes", new
        {
            nota = 5,
            comentario = "A avaliacao deve acionar a recomendacao."
        });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErroResponse>();
        Assert.Contains("Servico de recomendacao", payload!.Error);
        Assert.Single(_wireMock!.LogEntries);
    }

    private HttpClient CriarClienteAutenticado(Guid? usuarioId = null)
    {
        var client = _factory!.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CriarJwt(usuarioId ?? UsuarioId));
        return client;
    }

    private static string CriarJwt(Guid usuarioId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim("tipo_usuario", "cadastrado")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task PrepararBancoAsync()
    {
        var sql = string.Join(
            Environment.NewLine,
            await File.ReadAllTextAsync(Path.Combine(RaizRepositorio(), "src", "database", "00_supabase_compat.sql")),
            await File.ReadAllTextAsync(Path.Combine(RaizRepositorio(), "src", "database", "01_create_database.sql")),
            """
            INSERT INTO auth.users (id, aud, role, email, email_confirmed_at, raw_user_meta_data)
            VALUES
                ('10000000-0000-0000-0000-000000000001', 'authenticated', 'authenticated', 'teste1@spotlight.local', NOW(), '{"nome":"Usuario Teste 1"}'::jsonb),
                ('10000000-0000-0000-0000-000000000002', 'authenticated', 'authenticated', 'teste2@spotlight.local', NOW(), '{"nome":"Usuario Teste 2"}'::jsonb);

            INSERT INTO espetaculo (id_espetaculo, titulo, descricao, genero, duracao, status)
            VALUES (1, 'Mamma Mia!', 'Musical com repertorio ABBA.', 'Teatro Musical', 150, 'em_cartaz');
            """);

        await using var conexao = new NpgsqlConnection(_postgres.GetConnectionString());
        await conexao.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conexao);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string RaizRepositorio()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null && !File.Exists(Path.Combine(diretorio.FullName, "Spotlight.sln")))
        {
            diretorio = diretorio.Parent;
        }

        if (diretorio?.Parent?.Parent is null)
        {
            throw new InvalidOperationException("Nao foi possivel localizar a raiz do repositorio.");
        }

        return diretorio.Parent.Parent.FullName;
    }

    private sealed record AvaliacaoCriadaResponse(int Id, decimal Nota, string? Comentario, DateTime CreatedAt);
    private sealed record ErroResponse(string Error, string TraceId);
}
