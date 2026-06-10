using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;
using Spotlight.Usuario.Infrastructure.Geolocalizacao;
using Spotlight.Usuario.Presentation.Controllers;
using Xunit;
using UsuarioConflictException = Spotlight.Usuario.Domain.Entities.ConflictException;
using UsuarioNotFoundException = Spotlight.Usuario.Domain.Entities.NotFoundException;
using UsuarioValidationException = Spotlight.Usuario.Domain.Entities.ValidationException;

namespace Spotlight.Usuario.Tests;

public class UsuarioUseCasesTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task ObterUsuarioMe_QuandoExiste_MapeiaPerfil()
    {
        var usuarioId = Guid.NewGuid();
        var useCase = new ObterUsuarioMeUseCase(new FakeUsuarioRepository
        {
            Usuario = Usuario(usuarioId)
        });

        var resultado = await useCase.ExecutarAsync(usuarioId, Ct);

        Assert.Equal(usuarioId, resultado.Id);
        Assert.Equal("Ana Maria", resultado.Nome);
        Assert.Equal("cliente", resultado.TipoUsuario);
    }

    [Fact]
    public async Task ObterUsuarioMe_QuandoNaoExiste_LancaNotFound()
    {
        var useCase = new ObterUsuarioMeUseCase(new FakeUsuarioRepository { Usuario = null });

        await Assert.ThrowsAsync<UsuarioNotFoundException>(() => useCase.ExecutarAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task AtualizarUsuarioMe_ComDadosValidos_AtualizaEBuscaPerfil()
    {
        var usuarioId = Guid.NewGuid();
        var repositorio = new FakeUsuarioRepository
        {
            AtualizarResult = true,
            Usuario = Usuario(usuarioId)
        };
        var useCase = new AtualizarUsuarioMeUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(
            usuarioId,
            new UsuarioUpdateDto
            {
                Nome = "Ana Atualizada",
                DataNascimento = new DateOnly(1995, 5, 10)
            },
            Ct);

        Assert.True(repositorio.AtualizarCalled);
        Assert.Equal(usuarioId, repositorio.LastAtualizarId);
        Assert.Equal("Ana Maria", resultado.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AtualizarUsuarioMe_ComNomeVazio_LancaValidation(string nome)
    {
        var repositorio = new FakeUsuarioRepository();
        var useCase = new AtualizarUsuarioMeUseCase(repositorio);

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => useCase.ExecutarAsync(
            Guid.NewGuid(),
            new UsuarioUpdateDto { Nome = nome },
            Ct));

        Assert.Contains("nome", ex.Details.Keys);
        Assert.False(repositorio.AtualizarCalled);
    }

    [Fact]
    public async Task AtualizarUsuarioMe_ComNomeAcimaDoLimite_LancaValidation()
    {
        var useCase = new AtualizarUsuarioMeUseCase(new FakeUsuarioRepository());

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => useCase.ExecutarAsync(
            Guid.NewGuid(),
            new UsuarioUpdateDto { Nome = new string('A', 151) },
            Ct));

        Assert.Contains("nome", ex.Details.Keys);
    }

    [Fact]
    public async Task AtualizarUsuarioMe_QuandoRepositorioNaoAtualiza_LancaNotFound()
    {
        var useCase = new AtualizarUsuarioMeUseCase(new FakeUsuarioRepository { AtualizarResult = false });

        await Assert.ThrowsAsync<UsuarioNotFoundException>(() => useCase.ExecutarAsync(
            Guid.NewGuid(),
            new UsuarioUpdateDto { Nome = "Ana" },
            Ct));
    }

    [Fact]
    public void UsuarioUpdateDto_AceitaPayloadComLocalizacao_MasIgnoraOCampo()
    {
        // PUT /me com {"localizacao":"São Paulo"} desserializa sem erro
        // (System.Text.Json ignora propriedades desconhecidas por padrão), mas
        // o campo não existe no DTO — então o UPDATE no banco nunca toca a
        // coluna localizacao por esta rota. Atualização de localização vai por
        // PUT /me/localizacao (payload JSONB validado).
        const string payload = """{"nome":"Ana","localizacao":"São Paulo"}""";

        var dto = JsonSerializer.Deserialize<UsuarioUpdateDto>(
            payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(dto);
        Assert.Equal("Ana", dto!.Nome);
        Assert.Null(typeof(UsuarioUpdateDto).GetProperty("Localizacao"));
    }

    [Fact]
    public async Task ObterPreferencias_QuandoExiste_MapeiaPreferencias()
    {
        var useCase = new ObterPreferenciasUseCase(new FakeUsuarioRepository
        {
            Preferencias = new Preferencias
            {
                GenerosFavoritos = ["Drama"],
                CategoriasFavoritas = [1, 2],
                FaixaPrecoMax = 120
            }
        });

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), Ct);

        Assert.Equal(["Drama"], resultado.GenerosFavoritos);
        Assert.Equal([1, 2], resultado.CategoriasFavoritas);
        Assert.Equal(120, resultado.FaixaPrecoMax);
    }

    [Fact]
    public async Task AtualizarPreferencias_ComListasNulas_NormalizaParaListasVazias()
    {
        var repositorio = new FakeUsuarioRepository { AtualizarPreferenciasResult = true };
        var useCase = new AtualizarPreferenciasUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(
            Guid.NewGuid(),
            new PreferenciasDto
            {
                GenerosFavoritos = null!,
                CategoriasFavoritas = null!,
                FaixaPrecoMax = 0
            },
            Ct);

        Assert.Empty(resultado.GenerosFavoritos);
        Assert.Empty(resultado.CategoriasFavoritas);
        Assert.Empty(repositorio.LastPreferencias?.GenerosFavoritos ?? ["sentinel"]);
        Assert.Equal(0, resultado.FaixaPrecoMax);
    }

    [Fact]
    public async Task AtualizarPreferencias_ComPrecoNegativo_LancaValidation()
    {
        var repositorio = new FakeUsuarioRepository();
        var useCase = new AtualizarPreferenciasUseCase(repositorio);

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => useCase.ExecutarAsync(
            Guid.NewGuid(),
            new PreferenciasDto { FaixaPrecoMax = -1 },
            Ct));

        Assert.Contains("faixaPrecoMax", ex.Details.Keys);
        Assert.False(repositorio.AtualizarPreferenciasCalled);
    }

    [Fact]
    public async Task AtualizarPreferencias_QuandoUsuarioNaoExiste_LancaNotFound()
    {
        var useCase = new AtualizarPreferenciasUseCase(new FakeUsuarioRepository { AtualizarPreferenciasResult = false });

        await Assert.ThrowsAsync<UsuarioNotFoundException>(() => useCase.ExecutarAsync(
            Guid.NewGuid(),
            new PreferenciasDto { GenerosFavoritos = ["Drama"] },
            Ct));
    }

    [Fact]
    public async Task ListarAvaliacoesUsuario_NormalizaPaginacaoEMapeiaResultado()
    {
        var repositorio = new FakeAvaliacaoUsuarioRepository
        {
            Result = ([new AvaliacaoUsuario
            {
                Id = 9,
                Nota = 4,
                Comentario = "Bom",
                CreatedAt = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc),
                Espetaculo = new EspetaculoResumoUsuario { Id = 77, Titulo = "Musical X" }
            }], 1)
        };
        var useCase = new ListarAvaliacoesUsuarioUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), 0, -1, Ct);

        Assert.Equal(1, repositorio.LastPage);
        Assert.Equal(20, repositorio.LastPageSize);
        Assert.Equal(1, resultado.Pagination.TotalPages);
        Assert.Equal("Musical X", resultado.Data.Single().Espetaculo.Titulo);
    }

    [Fact]
    public async Task UsuarioLocalizacao_ComCoordenadasInvalidas_LancaValidation()
    {
        var service = new UsuarioLocalizacaoService(
            Array.Empty<NpgsqlDataSource>(),
            new FakeGeocodingClient());

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => service.AtualizarLocalizacaoAsync(
            Guid.NewGuid(),
            new AtualizarLocalizacaoRequest("Rua A", -91, 181, "place", "provider"),
            Ct));

        Assert.Contains("latitude", ex.Details.Keys);
        Assert.Contains("longitude", ex.Details.Keys);
    }

    [Fact]
    public async Task UsuarioLocalizacao_SemDataSourceConfigurado_LancaConflict()
    {
        var service = new UsuarioLocalizacaoService(
            Array.Empty<NpgsqlDataSource>(),
            new FakeGeocodingClient());

        await Assert.ThrowsAsync<UsuarioConflictException>(() => service.AtualizarLocalizacaoAsync(
            Guid.NewGuid(),
            new AtualizarLocalizacaoRequest("Rua A", -23.55, -46.63, "place", "provider"),
            Ct));
    }

    [Fact]
    public async Task TelemetriaService_ComEventoValido_ChamaRepositorio()
    {
        var usuarioId = Guid.NewGuid();
        var repositorio = new FakeTelemetriaRepository();
        var service = new TelemetriaService(repositorio);
        var properties = JsonDocument.Parse("""{"duration_ms":23000}""").RootElement.Clone();

        var eventId = await service.RegistrarAsync(
            usuarioId,
            new RegistrarTelemetriaRequest(
                EventId: null,
                EventType: "show_viewed",
                Source: "mobile",
                SessionId: "session-123",
                OccurredAt: new DateTimeOffset(2026, 6, 8, 12, 0, 0, TimeSpan.Zero),
                Screen: "detalhes",
                Entity: new TelemetriaEntityDto("espetaculo", "show-1"),
                Properties: properties,
                Device: null,
                AnonymousId: null),
            Ct);

        Assert.False(string.IsNullOrWhiteSpace(eventId));
        Assert.NotNull(repositorio.LastEvento);
        Assert.Equal(usuarioId, repositorio.LastEvento.UserId);
        Assert.Equal("show_viewed", repositorio.LastEvento.EventType);
        Assert.Equal("detalhes", repositorio.LastEvento.Screen);
        Assert.Equal("show-1", repositorio.LastEvento.Entity?.Id);
    }

    [Fact]
    public async Task TelemetriaService_ComPayloadInvalido_LancaValidationAntesDoRepositorio()
    {
        var repositorio = new FakeTelemetriaRepository();
        var service = new TelemetriaService(repositorio);

        var ex = await Assert.ThrowsAsync<UsuarioValidationException>(() => service.RegistrarAsync(
            usuarioId: null,
            new RegistrarTelemetriaRequest(
                EventId: null,
                EventType: "",
                Source: "",
                SessionId: "",
                OccurredAt: null,
                Screen: null,
                Entity: null,
                Properties: null,
                Device: null,
                AnonymousId: null),
            Ct));

        Assert.Contains("eventType", ex.Details.Keys);
        Assert.Contains("source", ex.Details.Keys);
        Assert.Contains("sessionId", ex.Details.Keys);
        Assert.Contains("usuario", ex.Details.Keys);
        Assert.Null(repositorio.LastEvento);
    }

    [Fact]
    public async Task TelemetriaController_ComEventoAnonimo_RetornaAccepted()
    {
        var service = new FakeTelemetriaService("evt-test");
        var controller = CriarTelemetriaController(service);

        var result = await controller.Registrar(
            new RegistrarTelemetriaRequest(
                EventId: null,
                EventType: "show_viewed",
                Source: "mobile",
                SessionId: "session-123",
                OccurredAt: null,
                Screen: "detalhes",
                Entity: new TelemetriaEntityDto("espetaculo", "show-1"),
                Properties: null,
                Device: null,
                AnonymousId: "anon-123"),
            Ct);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, response.StatusCode);
        var body = Assert.IsType<RegistrarTelemetriaResponse>(response.Value);
        Assert.Equal("evt-test", body.EventId);
        Assert.Equal("accepted", body.Status);
        Assert.Null(service.LastUsuarioId);
    }

    [Fact]
    public async Task TelemetriaController_ComUsuarioAutenticado_AssociaUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var service = new FakeTelemetriaService("evt-auth");
        var controller = CriarTelemetriaController(service, usuarioId);

        await controller.Registrar(
            new RegistrarTelemetriaRequest(
                EventId: "evt-auth",
                EventType: "screen_viewed",
                Source: "mobile",
                SessionId: "session-123",
                OccurredAt: null,
                Screen: "Catalogo",
                Entity: null,
                Properties: null,
                Device: null,
                AnonymousId: null),
            Ct);

        Assert.Equal(usuarioId, service.LastUsuarioId);
    }

    [Fact]
    public async Task TelemetriaController_SemServicoConfigurado_Retorna503()
    {
        var controller = CriarTelemetriaController(service: null);

        var result = await controller.Registrar(
            new RegistrarTelemetriaRequest(
                EventId: null,
                EventType: "show_viewed",
                Source: "mobile",
                SessionId: "session-123",
                OccurredAt: null,
                Screen: null,
                Entity: null,
                Properties: null,
                Device: null,
                AnonymousId: "anon-123"),
            Ct);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        var body = Assert.IsType<ErroDto>(response.Value);
        Assert.Equal("Telemetria nao configurada.", body.Error);
    }

    private static Domain.Entities.Usuario Usuario(Guid usuarioId) => new()
    {
        Id = usuarioId,
        Nome = "Ana Maria",
        Email = "ana@example.com",
        DataNascimento = new DateOnly(1995, 5, 10),
        Localizacao = "Sao Paulo",
        TipoUsuario = "cliente",
        DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public Domain.Entities.Usuario? Usuario { get; set; }
        public bool AtualizarResult { get; set; } = true;
        public Preferencias? Preferencias { get; set; }
        public bool AtualizarPreferenciasResult { get; set; } = true;
        public bool AtualizarCalled { get; private set; }
        public bool AtualizarPreferenciasCalled { get; private set; }
        public Guid LastAtualizarId { get; private set; }
        public Preferencias? LastPreferencias { get; private set; }

        public Task<Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Usuario);

        public Task<bool> AtualizarAsync(Guid id, UsuarioUpdateDto dados, CancellationToken cancellationToken)
        {
            AtualizarCalled = true;
            LastAtualizarId = id;
            return Task.FromResult(AtualizarResult);
        }

        public Task<Preferencias?> ObterPreferenciasAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Preferencias);

        public Task<bool> AtualizarPreferenciasAsync(
            Guid id,
            Preferencias dados,
            CancellationToken cancellationToken)
        {
            AtualizarPreferenciasCalled = true;
            LastPreferencias = dados;
            return Task.FromResult(AtualizarPreferenciasResult);
        }
    }

    private sealed class FakeAvaliacaoUsuarioRepository : IAvaliacaoUsuarioRepository
    {
        public (IReadOnlyList<AvaliacaoUsuario> Itens, int Total) Result { get; set; } = ([], 0);
        public int LastPage { get; private set; }
        public int LastPageSize { get; private set; }

        public Task<(IReadOnlyList<AvaliacaoUsuario> Itens, int Total)> ListarPorUsuarioAsync(
            Guid usuarioId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            LastPage = page;
            LastPageSize = pageSize;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeGeocodingClient : IGeocodingClient
    {
        public Task<GeocodingResult> ReverseGeocodeAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken) =>
            Task.FromResult(new GeocodingResult(
                FormattedAddress: "Endereco fake",
                Latitude: latitude,
                Longitude: longitude,
                PlaceId: "fake-place",
                Provider: "fake_provider"));
    }

    private sealed class FakeTelemetriaRepository : ITelemetriaRepository
    {
        public TelemetriaEvento? LastEvento { get; private set; }

        public Task InserirAsync(TelemetriaEvento evento, CancellationToken cancellationToken)
        {
            LastEvento = evento;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTelemetriaService(string eventId) : ITelemetriaService
    {
        public Guid? LastUsuarioId { get; private set; }
        public RegistrarTelemetriaRequest? LastRequest { get; private set; }

        public Task<string> RegistrarAsync(
            Guid? usuarioId,
            RegistrarTelemetriaRequest request,
            CancellationToken cancellationToken)
        {
            LastUsuarioId = usuarioId;
            LastRequest = request;
            return Task.FromResult(eventId);
        }
    }

    private static TelemetriaController CriarTelemetriaController(
        ITelemetriaService? service,
        Guid? usuarioId = null)
    {
        var services = new ServiceCollection();
        if (service is not null) services.AddSingleton(service);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        if (usuarioId is not null)
        {
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, usuarioId.Value.ToString())],
                authenticationType: "Test"));
        }

        return new TelemetriaController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}
