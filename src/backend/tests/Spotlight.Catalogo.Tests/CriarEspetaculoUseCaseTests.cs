using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;

namespace Spotlight.Catalogo.Tests;

public class CriarEspetaculoUseCaseTests
{
    [Fact]
    public async Task Rejeita_Sem_Titulo()
    {
        var useCase = new CriarEspetaculoUseCase(new EspetaculoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(new EspetaculoCreateDto(), CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("titulo"));
    }

    [Fact]
    public async Task Rejeita_Duracao_Zero_Ou_Negativa()
    {
        var useCase = new CriarEspetaculoUseCase(new EspetaculoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(
                new EspetaculoCreateDto { Titulo = "Peça", Duracao = 0 }, CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("duracao"));
    }

    [Fact]
    public async Task Rejeita_Status_Invalido()
    {
        var useCase = new CriarEspetaculoUseCase(new EspetaculoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(
                new EspetaculoCreateDto { Titulo = "Peça", Status = "publicado" }, CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("status"));
    }

    [Fact]
    public async Task Rejeita_Ator_Sem_AtorId()
    {
        var useCase = new CriarEspetaculoUseCase(new EspetaculoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(
                new EspetaculoCreateDto
                {
                    Titulo = "Peça",
                    Atores = [new AtorInputDto { Papel = "Hamlet" }]
                }, CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("atores"));
    }

    [Fact]
    public async Task Cria_E_Recarrega_Detalhe_Do_Repositorio()
    {
        var repo = new EspetaculoRepoFake
        {
            CriarResultado = 42,
            DetalheResultado = new EspetaculoDetalhe { Id = 42, Titulo = "Peça", Status = "em_breve" }
        };
        var useCase = new CriarEspetaculoUseCase(repo);

        var dto = await useCase.ExecutarAsync(
            new EspetaculoCreateDto { Titulo = "Peça" }, CancellationToken.None);

        Assert.Equal(42, dto.Id);
        Assert.Equal("Peça", repo.CriarRecebido?.Titulo);
    }
}
