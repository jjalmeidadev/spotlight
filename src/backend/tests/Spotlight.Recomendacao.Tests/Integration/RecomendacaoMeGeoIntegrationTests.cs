using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace Spotlight.Recomendacao.Tests.Integration;

/// <summary>
/// Testes de integração ponta a ponta do fluxo <c>GET /recomendacao/me</c>
/// (contrato externo; rota interna <c>/me</c>), combinando as DUAS dependências
/// no MESMO teste:
///
/// <list type="bullet">
///   <item><description><b>Possuída</b>: PostgreSQL real em container
///   (Testcontainers) — onde o serviço lê as recomendações e a localização do
///   usuário.</description></item>
///   <item><description><b>Consumida</b>: API externa de reverse geocoding,
///   stubada com WireMock.Net — usada para enriquecer a resposta com
///   <c>contextoGeografico</c>.</description></item>
/// </list>
///
/// Caixa-preta: a interação acontece só via HTTP (token + requisição) e as
/// asserções olham só para a resposta observável (status + corpo JSON);
/// nada de inspecionar banco ou estado interno do serviço.
/// </summary>
[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public class RecomendacaoMeGeoIntegrationTests(IntegrationFixture fixture)
{
    private const string RotaRecomendacoesMe = "/me";
    private const string RotaReverseGeocoding = "/reverse";
    // O serviço (NominatimGeocodingClient.ExtrairRotulo) prefere address.city ao
    // display_name — por isso o rótulo "identificável como vindo do stub" precisa
    // estar em address.city para realmente atravessar o fluxo ponta a ponta.
    private const string RotuloEsperadoDoStub = "Sao Paulo (stub WireMock)";

    [Fact]
    public async Task DeveRetornarRecomendacoesComContextoGeoQuandoGeocoderResponde200()
    {
        // Arrange: stub do geocoder respondendo 200 com um payload válido (estilo Nominatim/jsonv2)
        // e cliente HTTP autenticado com o JWT do usuário semeado (com localização).
        fixture.GeocodingMock.Reset();
        fixture.GeocodingMock
            .Given(Request.Create()
                .WithPath(RotaReverseGeocoding)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    place_id = 101,
                    display_name = "Avenida Paulista, Sao Paulo, SP, Brasil",
                    address = new { city = RotuloEsperadoDoStub, state = "SP" }
                }));

        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.GerarTokenUsuario());

        // Act: chama o endpoint real (Postgres real + geocoder stubado), só via HTTP.
        var resposta = await client.GetAsync(RotaRecomendacoesMe);

        // Assert: 200, lista populada (RF04 exige >= 5) e contexto geográfico preenchido
        // com o rótulo devolvido pelo stub — evidenciando que as DUAS dependências
        // participaram do MESMO fluxo.
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ListaRecomendacoesResponse>();

        Assert.NotNull(corpo);
        Assert.True(corpo!.Data.Count >= 5);
        Assert.All(corpo.Data, item => Assert.Equal(RotuloEsperadoDoStub, item.ContextoGeografico));
    }

    [Fact]
    public async Task DeveRetornarRecomendacoesSemContextoQuandoGeocoderIndisponivel()
    {
        // Arrange: geocoder fora do ar (503) — simula indisponibilidade da dependência consumida.
        fixture.GeocodingMock.Reset();
        fixture.GeocodingMock
            .Given(Request.Create()
                .WithPath(RotaReverseGeocoding)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.ServiceUnavailable));

        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", fixture.GerarTokenUsuario());

        // Act
        var resposta = await client.GetAsync(RotaRecomendacoesMe);

        // Assert: degradação graciosa (RNF03) — o endpoint NUNCA propaga o erro da
        // dependência consumida. Continua 200, com as recomendações vindas do
        // Postgres real, só que sem o enriquecimento (contextoGeografico == null).
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ListaRecomendacoesResponse>();

        Assert.NotNull(corpo);
        Assert.True(corpo!.Data.Count >= 5);
        Assert.All(corpo.Data, item => Assert.Null(item.ContextoGeografico));
    }

    // -----------------------------------------------------------------------
    // Modelos de desserialização do contrato observável (caixa-preta): só os
    // campos que os testes precisam afirmar — não reaproveita os DTOs internos
    // do serviço de propósito, para manter a checagem ancorada no contrato HTTP.
    // -----------------------------------------------------------------------
    private sealed class ListaRecomendacoesResponse
    {
        public List<RecomendacaoResponseItem> Data { get; init; } = [];
    }

    private sealed class RecomendacaoResponseItem
    {
        public string? ContextoGeografico { get; init; }
    }
}
