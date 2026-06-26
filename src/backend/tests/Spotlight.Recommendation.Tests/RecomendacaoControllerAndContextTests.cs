using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Recomendacao.Application.Dtos;
using Spotlight.Recomendacao.Application.Services;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;
using Spotlight.Recomendacao.Infrastructure.Auth;
using Spotlight.Recomendacao.Presentation.Controllers;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

public class RecomendacaoControllerAndContextTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public void UsuarioContextoHttp_ComNameIdentifierValido_RetornaGuid()
    {
        var usuarioId = Guid.NewGuid();
        var contexto = CriarContexto(new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()));

        Assert.Equal(usuarioId, contexto.UsuarioId);
    }

    [Fact]
    public void UsuarioContextoHttp_ComSubInvalido_RetornaNull()
    {
        var contexto = CriarContexto(new Claim("sub", "invalido"));

        Assert.Null(contexto.UsuarioId);
    }

    [Fact]
    public async Task RecomendacaoMeController_ComUsuarioValido_RetornaOkComLista()
    {
        var usuarioId = Guid.NewGuid();
        var service = new FakeRecomendacaoService
        {
            Resultado = new ListaDto<RecomendacaoDto>
            {
                Data = [new RecomendacaoDto { TipoAlgoritmo = "stub", Score = 0.9m }]
            }
        };
        var controller = new RecomendacaoMeController();
        var useCase = new ListarRecomendacoesMeUseCase(service);

        var result = await controller.Obter(5, useCase, new FakeUsuarioContexto(usuarioId), Ct);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ListaDto<RecomendacaoDto>>(ok.Value);
        Assert.Single(body.Data);
        Assert.Equal(usuarioId, service.LastUsuarioId);
        Assert.Equal(5, service.LastLimite);
    }

    [Fact]
    public async Task RecomendacaoMeController_SemUsuario_LancaValidation()
    {
        var controller = new RecomendacaoMeController();
        var useCase = new ListarRecomendacoesMeUseCase(new FakeRecomendacaoService());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            controller.Obter(5, useCase, new FakeUsuarioContexto(null), Ct));

        Assert.Contains("sub", ex.Details.Keys);
    }

    private static UsuarioContextoHttp CriarContexto(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };

        return new UsuarioContextoHttp(new HttpContextAccessor { HttpContext = httpContext });
    }

    private sealed class FakeUsuarioContexto(Guid? usuarioId) : IUsuarioContexto
    {
        public Guid? UsuarioId { get; } = usuarioId;
    }

    private sealed class FakeRecomendacaoService : IRecomendacaoService
    {
        public ListaDto<RecomendacaoDto> Resultado { get; init; } = new() { Data = [] };
        public Guid LastUsuarioId { get; private set; }
        public int? LastLimite { get; private set; }

        public Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
            Guid usuarioId,
            int? limite,
            CancellationToken cancellationToken)
        {
            LastUsuarioId = usuarioId;
            LastLimite = limite;
            return Task.FromResult(Resultado);
        }
    }
}
