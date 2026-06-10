using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;
using Spotlight.Catalogo.Application.Dtos;
using Xunit;

namespace Spotlight.Catalogo.Tests.Integration;

/// <summary>
/// Testes de integração de <c>POST /catalogo/espetaculos</c> (rota interna: <c>POST /espetaculos</c>).
/// </summary>
[Collection(CatalogoIntegrationCollection.Name)]
[Trait("Category", "Integration")]
public class EspetaculosPostIntegrationTests(CatalogoIntegrationFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Post_Espetaculos_ComAdministrador_CriaEspetaculoComRelacionamentos()
    {
        const string titulo = "Espetaculo Integracao Sucesso";

        var client = CriarClienteAutenticado("administrador");
        var body = new EspetaculoCreateDto
        {
            Titulo = titulo,
            Descricao = "Descricao valida para teste de integracao.",
            Duracao = 90,
            Status = "em_breve",
            CategoriaIds = [1],
            Atores =
            [
                new AtorInputDto
                {
                    AtorId = 1,
                    Papel = "Protagonista",
                    OrdemExibicao = 1
                }
            ]
        };

        var response = await client.PostAsJsonAsync("/espetaculos", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var detalhe = await response.Content.ReadFromJsonAsync<EspetaculoDetalheDto>(JsonOptions);
        Assert.NotNull(detalhe);
        Assert.Equal(titulo, detalhe.Titulo);
        Assert.Equal(90, detalhe.Duracao);
        Assert.Equal("em_breve", detalhe.Status);
        Assert.Contains(detalhe.Categorias, c => c.Id == 1);
        Assert.Contains(detalhe.Atores, a => a.Id == 1 && a.Papel == "Protagonista");

        await using var conexao = new NpgsqlConnection(fixture.ConnectionString);
        await conexao.OpenAsync();

        var idEspetaculo = await ObterIdPorTituloAsync(conexao, titulo);
        Assert.True(idEspetaculo > 0);
        Assert.Equal(1, await ContarRelacionamentoAsync(
            conexao,
            "SELECT COUNT(*) FROM espetaculo_categoria WHERE id_espetaculo = @id AND id_categoria = 1",
            idEspetaculo));
        Assert.Equal(1, await ContarRelacionamentoAsync(
            conexao,
            "SELECT COUNT(*) FROM espetaculo_ator WHERE id_espetaculo = @id AND id_ator = 1",
            idEspetaculo));
    }

    [Fact]
    public async Task Post_Espetaculos_ComUsuarioComum_Retorna403ENaoCriaEspetaculo()
    {
        const string titulo = "Espetaculo Integracao Negado";

        await using var conexao = new NpgsqlConnection(fixture.ConnectionString);
        await conexao.OpenAsync();

        var totalAntes = await ContarEspetaculosAsync(conexao);

        var client = CriarClienteAutenticado("cadastrado");
        var body = new EspetaculoCreateDto
        {
            Titulo = titulo,
            Duracao = 60,
            Status = "em_breve"
        };

        var response = await client.PostAsJsonAsync("/espetaculos", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var erro = await response.Content.ReadFromJsonAsync<ErroResposta>(JsonOptions);
        Assert.NotNull(erro);
        Assert.Contains("permissão", erro.Error, StringComparison.OrdinalIgnoreCase);

        var totalDepois = await ContarEspetaculosAsync(conexao);
        Assert.Equal(totalAntes, totalDepois);
        Assert.Equal(0, await ContarPorTituloAsync(conexao, titulo));
    }

    private HttpClient CriarClienteAutenticado(string tipoUsuario)
    {
        var client = fixture.Factory.CreateClient();
        var token = JwtTestHelper.CriarToken(tipoUsuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<int> ObterIdPorTituloAsync(NpgsqlConnection conexao, string titulo)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT id_espetaculo FROM espetaculo WHERE titulo = @titulo;",
            conexao);
        cmd.Parameters.AddWithValue("titulo", titulo);
        var resultado = await cmd.ExecuteScalarAsync();
        return resultado is int id ? id : 0;
    }

    private static async Task<int> ContarPorTituloAsync(NpgsqlConnection conexao, string titulo)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM espetaculo WHERE titulo = @titulo;",
            conexao);
        cmd.Parameters.AddWithValue("titulo", titulo);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private static async Task<int> ContarEspetaculosAsync(NpgsqlConnection conexao)
    {
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM espetaculo;", conexao);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private static async Task<int> ContarRelacionamentoAsync(
        NpgsqlConnection conexao, string sql, int idEspetaculo)
    {
        await using var cmd = new NpgsqlCommand(sql, conexao);
        cmd.Parameters.AddWithValue("id", idEspetaculo);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private sealed class ErroResposta
    {
        public string Error { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
    }
}
