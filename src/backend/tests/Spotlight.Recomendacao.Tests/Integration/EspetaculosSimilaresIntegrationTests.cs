using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Spotlight.Recomendacao.Tests.Integration;

/// <summary>
/// Testes de integração ponta a ponta de <c>GET /espetaculos/{id}/similares</c>
/// (rota pública — não requer autenticação) e da geração do documento OpenAPI.
/// Usa o MESMO Postgres real do <see cref="IntegrationFixture"/>, agora populado
/// com o seed oficial completo (<c>02_initial_data.sql</c>), incluindo as linhas
/// de <c>similaridade_item</c> que o cenário de sucesso explora.
///
/// Caixa-preta: a interação acontece só via HTTP, e as asserções olham só para a
/// resposta observável (status + corpo JSON).
/// </summary>
[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public class EspetaculosSimilaresIntegrationTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task DeveRetornarSimilaresOrdenadosPorScoreQuandoOrigemExiste()
    {
        // Arrange: espetáculo 1 ("Elis, a Musical") tem 3 similares no seed oficial,
        // com scores 0.8945 (id 6), 0.8612 (id 9) e 0.7421 (id 3) — nessa ordem.
        var client = fixture.CreateClient();

        // Act: rota pública — sem token.
        var resposta = await client.GetAsync("/espetaculos/1/similares");

        // Assert: 200, lista populada e ordenada por score decrescente — prova de
        // que o JOIN com `espetaculo` e a leitura de `similaridade_item` (dependência
        // possuída) percorreram o caminho real, ponta a ponta.
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ListaSimilaresResponse>();

        Assert.NotNull(corpo);
        Assert.True(corpo!.Data.Count >= 3);
        Assert.Equal(6, corpo.Data[0].Espetaculo.Id);
        Assert.Equal(9, corpo.Data[1].Espetaculo.Id);
        Assert.Equal(3, corpo.Data[2].Espetaculo.Id);
        Assert.True(corpo.Data[0].Score > corpo.Data[1].Score);
        Assert.True(corpo.Data[1].Score > corpo.Data[2].Score);
        Assert.All(corpo.Data, item => Assert.False(string.IsNullOrWhiteSpace(item.Espetaculo.Titulo)));
    }

    [Fact]
    public async Task DeveRetornar404QuandoEspetaculoDeOrigemNaoExiste()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/espetaculos/999999/similares");

        // Assert: 404 traduzido pelo ErrorHandlingMiddleware a partir da
        // NotFoundException lançada pelo caso de uso quando a origem não existe.
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ErroResponse>();
        Assert.NotNull(corpo);
        Assert.Contains("999999", corpo!.Error);
        Assert.False(string.IsNullOrWhiteSpace(corpo.TraceId));
    }

    [Fact]
    public async Task DeveGerarDocumentoOpenApiDistinguindoRotasPublicasEAutenticadas()
    {
        // O documento OpenAPI é gerado sob demanda nesta requisição, exercitando
        // o AuthorizeOperationFilter para cada operação registrada — incluindo a
        // rota pública (sem [Authorize], sem cadeado) e a autenticada (com
        // [Authorize], cadeado + security requirement).
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var documento = await resposta.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var paths = documento.GetProperty("paths");

        var operacaoPublica = paths.GetProperty("/espetaculos/{id}/similares").GetProperty("get");
        var operacaoAutenticada = paths.GetProperty("/me").GetProperty("get");

        Assert.False(operacaoPublica.TryGetProperty("security", out _));
        Assert.True(operacaoAutenticada.TryGetProperty("security", out var security));
        Assert.True(security.GetArrayLength() > 0);
    }

    private sealed class ListaSimilaresResponse
    {
        public List<SimilarResponseItem> Data { get; init; } = [];
    }

    private sealed class SimilarResponseItem
    {
        public EspetaculoResponseItem Espetaculo { get; init; } = new();
        public decimal Score { get; init; }
    }

    private sealed class EspetaculoResponseItem
    {
        public int Id { get; init; }
        public string Titulo { get; init; } = string.Empty;
    }

    private sealed class ErroResponse
    {
        public string Error { get; init; } = string.Empty;
        public string TraceId { get; init; } = string.Empty;
    }
}
