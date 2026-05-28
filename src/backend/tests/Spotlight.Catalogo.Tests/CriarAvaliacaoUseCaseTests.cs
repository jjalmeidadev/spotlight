using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;

namespace Spotlight.Catalogo.Tests;

public class CriarAvaliacaoUseCaseTests
{
    private static CriarAvaliacaoUseCase Construir(
        EspetaculoRepoFake espetaculoRepo, AvaliacaoRepoFake avaliacaoRepo)
        => new(avaliacaoRepo, espetaculoRepo);

    [Fact]
    public async Task Rejeita_Quando_Nota_Ausente()
    {
        var useCase = Construir(new EspetaculoRepoFake(), new AvaliacaoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(12, Guid.NewGuid(), new AvaliacaoCreateDto(), CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("nota"));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.01)]
    public async Task Rejeita_Quando_Nota_Fora_De_Faixa(decimal nota)
    {
        var useCase = Construir(new EspetaculoRepoFake(), new AvaliacaoRepoFake());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(12, Guid.NewGuid(),
                new AvaliacaoCreateDto { Nota = nota }, CancellationToken.None));

        Assert.True(ex.Details.ContainsKey("nota"));
    }

    [Fact]
    public async Task Retorna_NotFound_Quando_Espetaculo_Nao_Existe()
    {
        var useCase = Construir(
            new EspetaculoRepoFake { ExisteResultado = false }, new AvaliacaoRepoFake());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(999, Guid.NewGuid(),
                new AvaliacaoCreateDto { Nota = 4 }, CancellationToken.None));
    }

    [Fact]
    public async Task Cria_Avaliacao_E_Retorna_Dto()
    {
        var useCase = Construir(new EspetaculoRepoFake(), new AvaliacaoRepoFake());

        var dto = await useCase.ExecutarAsync(
            12, Guid.NewGuid(),
            new AvaliacaoCreateDto { Nota = 4.5m, Comentario = "Imperdível" },
            CancellationToken.None);

        Assert.Equal(41, dto.Id);
        Assert.Equal(4.5m, dto.Nota);
        Assert.Equal("Imperdível", dto.Comentario);
    }
}
