using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

/// <summary>
/// Testes de integração do esqueleto do serviço de recomendação: garantem que o
/// serviço sobe e que os endpoints respondem sem depender de um banco real.
/// </summary>
public class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", string.Empty);
        this.factory = factory;
    }

    [Fact]
    public async Task Get_Health_Returns_200()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Ping_Returns_200()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_HealthReady_Returns_503_SemBanco()
    {
        // Sem connection string configurada (ambiente de teste), o readiness
        // deve falhar — evita falso positivo de "pronto para tráfego".
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
