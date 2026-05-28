using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;

namespace Spotlight.Catalogo.Tests;

/// <summary>
/// Testes unitários da lógica de negócio da listagem de espetáculos
/// (normalização de paginação/ordenação, cálculo de totalPages e mapeamento),
/// usando um repositório falso — sem tocar no banco.
/// </summary>
public class ListarEspetaculosUseCaseTests
{
    /// <summary>Repositório em memória que registra o filtro recebido e devolve dados fixos.</summary>
    private sealed class RepositorioFake : IEspetaculoRepository
    {
        public ListarEspetaculosFiltro? FiltroRecebido { get; private set; }
        public IReadOnlyList<Espetaculo> Itens { get; init; } = [];
        public int Total { get; init; }

        public Task<(IReadOnlyList<Espetaculo> Itens, int Total)> ListarAsync(
            ListarEspetaculosFiltro filtro, CancellationToken cancellationToken)
        {
            FiltroRecebido = filtro;
            return Task.FromResult((Itens, Total));
        }

        // Stubs dos demais métodos da interface — não usados neste arquivo de testes.
        public Task<EspetaculoDetalhe?> ObterDetalheAsync(int id, CancellationToken ct)
            => throw new NotImplementedException();
        public Task<bool> ExisteAsync(int id, CancellationToken ct) => throw new NotImplementedException();
        public Task<int> CriarAsync(EspetaculoCreateDto dados, CancellationToken ct)
            => throw new NotImplementedException();
        public Task<bool> AtualizarAsync(int id, EspetaculoUpdateDto dados, CancellationToken ct)
            => throw new NotImplementedException();
        public Task<bool> ExcluirAsync(int id, CancellationToken ct) => throw new NotImplementedException();
    }

    [Fact]
    public async Task Normaliza_Pagina_PageSize_E_Ordenacao_Invalidos()
    {
        var repo = new RepositorioFake();
        var useCase = new ListarEspetaculosUseCase(repo);

        await useCase.ExecutarAsync(
            new ListarEspetaculosFiltro { Page = 0, PageSize = 500, OrdenarPor = "campo_invalido" },
            CancellationToken.None);

        Assert.NotNull(repo.FiltroRecebido);
        Assert.Equal(1, repo.FiltroRecebido!.Page);        // page < 1 -> 1
        Assert.Equal(100, repo.FiltroRecebido.PageSize);   // pageSize > 100 -> 100
        Assert.Equal("popularidade", repo.FiltroRecebido.OrdenarPor); // inválido -> default
    }

    [Fact]
    public async Task PageSize_Zero_Usa_Default_20()
    {
        var repo = new RepositorioFake();
        var useCase = new ListarEspetaculosUseCase(repo);

        await useCase.ExecutarAsync(
            new ListarEspetaculosFiltro { PageSize = 0 }, CancellationToken.None);

        Assert.Equal(20, repo.FiltroRecebido!.PageSize);
    }

    [Fact]
    public async Task Calcula_TotalPages_Arredondando_Para_Cima()
    {
        var repo = new RepositorioFake { Total = 137 };
        var useCase = new ListarEspetaculosUseCase(repo);

        var resultado = await useCase.ExecutarAsync(
            new ListarEspetaculosFiltro { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.Equal(137, resultado.Pagination.TotalItems);
        Assert.Equal(7, resultado.Pagination.TotalPages); // ceil(137/20) = 7
        Assert.Equal(1, resultado.Pagination.Page);
        Assert.Equal(20, resultado.Pagination.PageSize);
    }

    [Fact]
    public async Task TotalPages_Zero_Quando_Sem_Resultados()
    {
        var repo = new RepositorioFake { Total = 0 };
        var useCase = new ListarEspetaculosUseCase(repo);

        var resultado = await useCase.ExecutarAsync(
            new ListarEspetaculosFiltro(), CancellationToken.None);

        Assert.Equal(0, resultado.Pagination.TotalPages);
        Assert.Empty(resultado.Data);
    }

    [Fact]
    public async Task Mapeia_Campos_Da_Entidade_Para_O_Dto()
    {
        var repo = new RepositorioFake
        {
            Total = 1,
            Itens =
            [
                new Espetaculo
                {
                    Id = 12,
                    Titulo = "O Auto da Compadecida",
                    Genero = "Comédia",
                    Status = "em_cartaz",
                    FaixaEtaria = "Livre",
                    ClassificacaoIndicativa = "L",
                    Duracao = 120,
                    NotaMedia = 4.7m,
                    Popularidade = 8123.5m
                }
            ]
        };
        var useCase = new ListarEspetaculosUseCase(repo);

        var resultado = await useCase.ExecutarAsync(
            new ListarEspetaculosFiltro(), CancellationToken.None);

        var dto = Assert.Single(resultado.Data);
        Assert.Equal(12, dto.Id);
        Assert.Equal("O Auto da Compadecida", dto.Titulo);
        Assert.Equal("Comédia", dto.Genero);
        Assert.Equal("em_cartaz", dto.Status);
        Assert.Equal("Livre", dto.FaixaEtaria);
        Assert.Equal("L", dto.ClassificacaoIndicativa);
        Assert.Equal(120, dto.Duracao);
        Assert.Equal(4.7m, dto.NotaMedia);
        Assert.Equal(8123.5m, dto.Popularidade);
    }
}
