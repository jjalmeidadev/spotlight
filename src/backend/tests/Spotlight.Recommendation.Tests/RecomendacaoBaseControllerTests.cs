using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Presentation.Controllers;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

public class RecomendacaoBaseControllerTests
{
    [Fact]
    public void Obter_DeveRetornarContratoBaseDaApiDeRecomendacao()
    {
        var controller = new RecomendacaoBaseController();

        var resultado = controller.Obter();

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var contrato = Assert.IsType<RecomendacaoBaseDto>(ok.Value);
        Assert.Equal("recomendacao", contrato.Service);
        Assert.Equal("ok", contrato.Status);
        Assert.Equal("v1", contrato.Version);
        Assert.Contains(contrato.Endpoints, endpoint =>
            endpoint.Method == "GET" &&
            endpoint.Path == "/recomendacao/me" &&
            endpoint.Auth == "cadastrado");
        Assert.Contains(contrato.Endpoints, endpoint =>
            endpoint.Method == "GET" &&
            endpoint.Path == "/recomendacao/espetaculos/{id}/similares" &&
            endpoint.Auth == "publico");
    }
}
