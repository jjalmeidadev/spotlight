using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Spotlight.Gateway.Tests;

/// <summary>
/// Testes de integração do esqueleto do gateway: garantem que ele sobe e que os
/// endpoints próprios (health/ping) respondem sem depender dos serviços downstream.
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
