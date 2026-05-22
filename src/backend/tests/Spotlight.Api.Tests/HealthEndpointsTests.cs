using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Spotlight.Api.Tests;

/// <summary>
/// Testes de integração do esqueleto: garantem que a API sobe e que o
/// endpoint de liveness responde 200 sem depender de um banco real.
/// </summary>
public class HealthEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
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
}
