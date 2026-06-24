using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;
using Spotlight.Usuario.Infrastructure.Auth;
using Spotlight.Usuario.Presentation.Controllers;
using Xunit;
using UsuarioValidationException = Spotlight.Usuario.Domain.Entities.ValidationException;

namespace Spotlight.Usuario.Tests;

public class UsuarioContextoEControllerTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public void UsuarioContextoHttp_ComClaimsDiretas_MapeiaIdentidade()
    {
        var usuarioId = Guid.NewGuid();
        var contexto = CriarContexto(
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, "ana@example.com"),
            new Claim(ClaimTypes.Name, "Ana Direta"),
            new Claim("dataNascimento", "1995-05-10"));

        Assert.Equal(usuarioId, contexto.UsuarioId);
        Assert.Equal("ana@example.com", contexto.Email);
        Assert.Equal("Ana Direta", contexto.Nome);
        Assert.Equal(new DateOnly(1995, 5, 10), contexto.DataNascimento);
    }

    [Fact]
    public void UsuarioContextoHttp_ComMetadataPriorizaNomeEDataDoJson()
    {
        var contexto = CriarContexto(
            new Claim("user_metadata", """{"nome":"Ana Metadata","dataNascimento":"1990-01-02"}"""),
            new Claim(ClaimTypes.Name, "Ana Claim"));

        Assert.Equal("Ana Metadata", contexto.Nome);
        Assert.Equal(new DateOnly(1990, 1, 2), contexto.DataNascimento);
    }

    [Fact]
    public void UsuarioContextoHttp_ComMetadataInvalido_RetornaFallbacks()
    {
        var contexto = CriarContexto(
            new Claim("user_metadata", "{"),
            new Claim("name", "Nome Fallback"),
            new Claim("dataNascimento", "invalida"));

        Assert.Equal("Nome Fallback", contexto.Nome);
        Assert.Null(contexto.DataNascimento);
    }

    [Fact]
    public async Task UsuarioMeController_ObterPreferencias_GarantePerfilQuandoEmailExiste()
    {
        var usuarioId = Guid.NewGuid();
        var repositorio = new FakeUsuarioRepository
        {
            Preferencias = new Preferencias
            {
                GenerosFavoritos = ["Drama"],
                CategoriasFavoritas = [1],
                FaixaPrecoMax = 120
            }
        };
        var controller = new UsuarioMeController();
        var useCase = new ObterPreferenciasUseCase(repositorio);

        var result = await controller.ObterPreferencias(
            useCase,
            repositorio,
            new FakeUsuarioContexto(usuarioId, "ana@example.com", "Ana", new DateOnly(1995, 5, 10)),
            Ct);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<PreferenciasDto>(ok.Value);
        Assert.Equal(["Drama"], body.GenerosFavoritos);
        Assert.Equal(usuarioId, repositorio.LastGarantirPerfilId);
        Assert.Equal("ana@example.com", repositorio.LastGarantirPerfilEmail);
    }

    [Fact]
    public async Task UsuarioMeController_AtualizarPreferencias_SemEmailNaoGarantePerfil()
    {
        var usuarioId = Guid.NewGuid();
        var repositorio = new FakeUsuarioRepository { AtualizarPreferenciasResult = true };
        var controller = new UsuarioMeController();
        var useCase = new AtualizarPreferenciasUseCase(repositorio);

        var result = await controller.AtualizarPreferencias(
            new PreferenciasDto { GenerosFavoritos = ["Musical"], CategoriasFavoritas = [2], FaixaPrecoMax = 90 },
            useCase,
            repositorio,
            new FakeUsuarioContexto(usuarioId, email: null, nome: null, dataNascimento: null),
            Ct);

        Assert.IsType<OkObjectResult>(result);
        Assert.False(repositorio.GarantirPerfilCalled);
        Assert.True(repositorio.AtualizarPreferenciasCalled);
    }

    [Fact]
    public async Task UsuarioMeController_SemUsuario_LancaValidation()
    {
        var repositorio = new FakeUsuarioRepository();
        var controller = new UsuarioMeController();
        var useCase = new ObterPreferenciasUseCase(repositorio);

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => controller.ObterPreferencias(
            useCase,
            repositorio,
            new FakeUsuarioContexto(null, "ana@example.com", "Ana", null),
            Ct));

        Assert.Contains("sub", ex.Details.Keys);
        Assert.False(repositorio.GarantirPerfilCalled);
    }

    private static UsuarioContextoHttp CriarContexto(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };

        return new UsuarioContextoHttp(new HttpContextAccessor { HttpContext = httpContext });
    }

    private sealed class FakeUsuarioContexto(
        Guid? usuarioId,
        string? email,
        string? nome,
        DateOnly? dataNascimento) : IUsuarioContexto
    {
        public Guid? UsuarioId { get; } = usuarioId;
        public string? Email { get; } = email;
        public string? Nome { get; } = nome;
        public DateOnly? DataNascimento { get; } = dataNascimento;
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public Preferencias? Preferencias { get; init; }
        public bool AtualizarPreferenciasResult { get; init; } = true;
        public bool GarantirPerfilCalled { get; private set; }
        public bool AtualizarPreferenciasCalled { get; private set; }
        public Guid LastGarantirPerfilId { get; private set; }
        public string? LastGarantirPerfilEmail { get; private set; }

        public Task<Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Domain.Entities.Usuario?>(null);

        public Task GarantirPerfilAsync(
            Guid id,
            string email,
            string? nome,
            DateOnly? dataNascimento,
            CancellationToken cancellationToken)
        {
            GarantirPerfilCalled = true;
            LastGarantirPerfilId = id;
            LastGarantirPerfilEmail = email;
            return Task.CompletedTask;
        }

        public Task<bool> AtualizarAsync(
            Guid id,
            UsuarioUpdateDto dados,
            CancellationToken cancellationToken) => Task.FromResult(false);

        public Task<Domain.Entities.Usuario?> PromoverAdministradorPorEmailAsync(
            string email,
            CancellationToken cancellationToken) => Task.FromResult<Domain.Entities.Usuario?>(null);

        public Task<Preferencias?> ObterPreferenciasAsync(
            Guid id,
            CancellationToken cancellationToken) => Task.FromResult(Preferencias);

        public Task<bool> AtualizarPreferenciasAsync(
            Guid id,
            Preferencias dados,
            CancellationToken cancellationToken)
        {
            AtualizarPreferenciasCalled = true;
            return Task.FromResult(AtualizarPreferenciasResult);
        }
    }
}
