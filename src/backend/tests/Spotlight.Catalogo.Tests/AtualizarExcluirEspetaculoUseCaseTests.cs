using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;

namespace Spotlight.Catalogo.Tests;

public class AtualizarExcluirEspetaculoUseCaseTests
{
    [Fact]
    public async Task Atualizar_NotFound_Quando_Id_Inexiste()
    {
        var repo = new EspetaculoRepoFake { AtualizarResultado = false };
        var useCase = new AtualizarEspetaculoUseCase(repo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(999, new EspetaculoUpdateDto { Titulo = "Novo" }, CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_Aplica_E_Retorna_Detalhe()
    {
        var repo = new EspetaculoRepoFake
        {
            AtualizarResultado = true,
            DetalheResultado = new EspetaculoDetalhe { Id = 12, Titulo = "Novo Título", Status = "em_cartaz" }
        };
        var useCase = new AtualizarEspetaculoUseCase(repo);

        var dto = await useCase.ExecutarAsync(
            12, new EspetaculoUpdateDto { Titulo = "Novo Título" }, CancellationToken.None);

        Assert.Equal(12, dto.Id);
        Assert.Equal("Novo Título", dto.Titulo);
        Assert.Equal((12, "Novo Título"), (repo.AtualizarRecebido?.Id, repo.AtualizarRecebido?.Dados.Titulo));
    }

    [Fact]
    public async Task Excluir_NotFound_Quando_Id_Inexiste()
    {
        var repo = new EspetaculoRepoFake { ExcluirResultado = false };
        var useCase = new ExcluirEspetaculoUseCase(repo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task Excluir_Passa_Id_Ao_Repositorio()
    {
        var repo = new EspetaculoRepoFake { ExcluirResultado = true };
        var useCase = new ExcluirEspetaculoUseCase(repo);

        await useCase.ExecutarAsync(12, CancellationToken.None);

        Assert.Equal(12, repo.ExcluirRecebido);
    }
}
