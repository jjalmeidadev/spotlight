using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

public class ListarRecomendacoesMeUseCaseTests
{
    private sealed class RepoFake : IRecomendacaoRepository
    {
        public IReadOnlyList<Spotlight.Recomendacao.Domain.Entities.Recomendacao> Itens { get; set; } = [];
        public (Guid Id, int Limite)? Recebido { get; private set; }

        public Task<IReadOnlyList<Spotlight.Recomendacao.Domain.Entities.Recomendacao>> ListarPorUsuarioAsync(
            Guid usuarioId, int limite, CancellationToken cancellationToken)
        {
            Recebido = (usuarioId, limite);
            return Task.FromResult(Itens);
        }
    }

    [Fact]
    public async Task Limite_Null_Vira_Default_10()
    {
        var repo = new RepoFake();
        var useCase = new ListarRecomendacoesMeUseCase(repo);

        await useCase.ExecutarAsync(Guid.NewGuid(), null, CancellationToken.None);

        Assert.Equal(10, repo.Recebido?.Limite);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    [InlineData(100, 50)]   // teto = 50
    [InlineData(25, 25)]    // valor normal preservado
    public async Task Limite_Normalizado_Em_1_A_50(int entrada, int esperado)
    {
        var repo = new RepoFake();
        var useCase = new ListarRecomendacoesMeUseCase(repo);

        await useCase.ExecutarAsync(Guid.NewGuid(), entrada, CancellationToken.None);

        Assert.Equal(esperado, repo.Recebido?.Limite);
    }

    [Fact]
    public async Task Lista_Vazia_Retorna_Envelope_Sem_Itens()
    {
        var useCase = new ListarRecomendacoesMeUseCase(new RepoFake());

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), 10, CancellationToken.None);

        Assert.Empty(resultado.Data);
    }

    [Fact]
    public async Task Mapeia_Recomendacao_Para_Dto_Com_Espetaculo()
    {
        var repo = new RepoFake
        {
            Itens =
            [
                new Spotlight.Recomendacao.Domain.Entities.Recomendacao
                {
                    Score = 0.93m,
                    TipoAlgoritmo = "SVD",
                    Motivo = "Por que você gostou de comédias",
                    DataGeracao = new DateTime(2026, 5, 25),
                    Espetaculo = new EspetaculoRecomendado
                    {
                        Id = 12, Titulo = "O Auto", Genero = "Comédia",
                        Status = "em_cartaz", NotaMedia = 4.7m
                    }
                }
            ]
        };
        var useCase = new ListarRecomendacoesMeUseCase(repo);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), 10, CancellationToken.None);

        var item = Assert.Single(resultado.Data);
        Assert.Equal(0.93m, item.Score);
        Assert.Equal("SVD", item.TipoAlgoritmo);
        Assert.Equal(12, item.Espetaculo.Id);
        Assert.Equal("Comédia", item.Espetaculo.Genero);
    }
}
