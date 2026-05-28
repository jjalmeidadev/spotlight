using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;

namespace Spotlight.Catalogo.Tests;

public class ObterEspetaculoUseCaseTests
{
    [Fact]
    public async Task Lanca_NotFound_Quando_Detalhe_Nao_Existe()
    {
        var repo = new EspetaculoRepoFake { DetalheResultado = null };
        var useCase = new ObterEspetaculoUseCase(repo);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecutarAsync(999, CancellationToken.None));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task Mapeia_Detalhe_Para_Dto_Incluindo_Relacionamentos()
    {
        var repo = new EspetaculoRepoFake
        {
            DetalheResultado = new EspetaculoDetalhe
            {
                Id = 12, Titulo = "O Auto", Descricao = "Adaptação...", Genero = "Comédia",
                Status = "em_cartaz", Duracao = 120, NotaMedia = 4.7m, Popularidade = 8123.5m,
                CreatedAt = new DateTime(2026, 5, 1), UpdatedAt = new DateTime(2026, 5, 20),
                Categorias = [new CategoriaResumo { Id = 3, Nome = "Teatro Popular" }],
                Atores = [new AtorElenco { Id = 5, Nome = "João Silva", Papel = "João Grilo", OrdemExibicao = 1 }],
                Sessoes = [new Sessao
                {
                    Id = 88, Data = new DateOnly(2026, 6, 10), Horario = new TimeOnly(20, 0),
                    PrecoMinimo = 40m, PrecoMaximo = 120m, Disponibilidade = 230,
                    Teatro = new TeatroResumo { Id = 2, Nome = "Teatro Aventura", Cidade = "Campinas", Estado = "SP" }
                }]
            }
        };
        var useCase = new ObterEspetaculoUseCase(repo);

        var dto = await useCase.ExecutarAsync(12, CancellationToken.None);

        Assert.Equal(12, dto.Id);
        Assert.Equal("Comédia", dto.Genero);
        Assert.Single(dto.Categorias);
        Assert.Equal("Teatro Popular", dto.Categorias[0].Nome);
        Assert.Single(dto.Atores);
        Assert.Equal("João Grilo", dto.Atores[0].Papel);
        var sessao = Assert.Single(dto.Sessoes);
        Assert.Equal("2026-06-10", sessao.Data);
        Assert.Equal("20:00", sessao.Horario);
        Assert.Equal("Teatro Aventura", sessao.Teatro.Nome);
    }
}
