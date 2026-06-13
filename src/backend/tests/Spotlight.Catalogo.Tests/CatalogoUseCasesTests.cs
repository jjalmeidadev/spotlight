using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;
using Xunit;
using CatalogoNotFoundException = Spotlight.Catalogo.Domain.Entities.NotFoundException;
using CatalogoValidationException = Spotlight.Catalogo.Domain.Entities.ValidationException;

namespace Spotlight.Catalogo.Tests;

public class CatalogoUseCasesTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task ListarEspetaculos_NormalizaFiltrosEPaginacao_MapeiaResultado()
    {
        var repositorio = new FakeEspetaculoRepository
        {
            ListarResult = ([new Espetaculo
            {
                Id = 10,
                Titulo = "Hamlet",
                Genero = "Drama",
                Status = "em_cartaz",
                FaixaEtaria = "14+",
                ClassificacaoIndicativa = "14",
                Duracao = 120,
                NotaMedia = 4.7m,
                Popularidade = 98.5m
            }], 101)
        };
        var useCase = new ListarEspetaculosUseCase(repositorio);
        var filtro = new ListarEspetaculosFiltro { Page = 0, PageSize = 500, OrdenarPor = "invalido" };

        var resultado = await useCase.ExecutarAsync(filtro, Ct);

        Assert.Equal(1, repositorio.LastFiltro?.Page);
        Assert.Equal(100, repositorio.LastFiltro?.PageSize);
        Assert.Equal("popularidade", repositorio.LastFiltro?.OrdenarPor);
        Assert.Equal(2, resultado.Pagination.TotalPages);
        var item = Assert.Single(resultado.Data);
        Assert.Equal("Hamlet", item.Titulo);
        Assert.Equal(4.7m, item.NotaMedia);
    }

    [Fact]
    public async Task ObterEspetaculo_QuandoExiste_MapeiaDetalheCompleto()
    {
        var repositorio = new FakeEspetaculoRepository { Detalhe = DetalheCompleto() };
        var useCase = new ObterEspetaculoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(7, Ct);

        Assert.Equal(7, resultado.Id);
        Assert.Equal("O Auto da Compadecida", resultado.Titulo);
        Assert.Equal("Comedia", resultado.Categorias.Single().Nome);
        Assert.Equal("Joao Grilo", resultado.Atores.Single().Papel);
        Assert.Equal("2026-06-15", resultado.Sessoes.Single().Data);
        Assert.Equal("20:30", resultado.Sessoes.Single().Horario);
        Assert.Equal("Teatro Alfa", resultado.Sessoes.Single().Teatro.Nome);
    }

    [Fact]
    public async Task ObterEspetaculo_QuandoNaoExiste_LancaNotFound()
    {
        var useCase = new ObterEspetaculoUseCase(new FakeEspetaculoRepository { Detalhe = null });

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(404, Ct));
    }

    [Fact]
    public async Task CriarEspetaculo_ComDadosValidos_CriaEBuscaDetalhe()
    {
        var repositorio = new FakeEspetaculoRepository { CriarResult = 7, Detalhe = DetalheCompleto() };
        var useCase = new CriarEspetaculoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(new EspetaculoCreateDto
        {
            Titulo = "O Auto da Compadecida",
            Duracao = 120,
            Status = "em_cartaz",
            Atores = [new AtorInputDto { AtorId = 1, Papel = "Joao Grilo" }]
        }, Ct);

        Assert.Equal(1, repositorio.CriarCalls);
        Assert.Equal(7, repositorio.LastObterDetalheId);
        Assert.Equal("O Auto da Compadecida", resultado.Titulo);
    }

    [Fact]
    public async Task CriarEspetaculo_ComCamposInvalidos_AcumulaErrosDeValidacao()
    {
        var useCase = new CriarEspetaculoUseCase(new FakeEspetaculoRepository());

        var ex = await Assert.ThrowsAsync<CatalogoValidationException>(() => useCase.ExecutarAsync(
            new EspetaculoCreateDto
            {
                Titulo = " ",
                Duracao = 0,
                Status = "rascunho",
                Atores = [new AtorInputDto()]
            }, Ct));

        Assert.Contains("titulo", ex.Details.Keys);
        Assert.Contains("duracao", ex.Details.Keys);
        Assert.Contains("status", ex.Details.Keys);
        Assert.Contains("atores", ex.Details.Keys);
    }

    [Fact]
    public async Task AtualizarEspetaculo_QuandoRepositorioNaoAtualiza_LancaNotFound()
    {
        var useCase = new AtualizarEspetaculoUseCase(new FakeEspetaculoRepository { AtualizarResult = false });

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(
            99,
            new EspetaculoUpdateDto { Titulo = "Novo titulo" },
            Ct));
    }

    [Fact]
    public async Task AtualizarEspetaculo_ComTituloMuitoLongo_NaoChamaRepositorio()
    {
        var repositorio = new FakeEspetaculoRepository();
        var useCase = new AtualizarEspetaculoUseCase(repositorio);

        await Assert.ThrowsAsync<CatalogoValidationException>(() => useCase.ExecutarAsync(
            7,
            new EspetaculoUpdateDto { Titulo = new string('A', 201) },
            Ct));

        Assert.Equal(0, repositorio.AtualizarCalls);
    }

    [Fact]
    public async Task ExcluirEspetaculo_QuandoNaoExiste_LancaNotFound()
    {
        var useCase = new ExcluirEspetaculoUseCase(new FakeEspetaculoRepository { ExcluirResult = false });

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(22, Ct));
    }

    [Fact]
    public async Task CriarAvaliacao_ComNotaValida_RetornaAvaliacaoCriada()
    {
        var avaliacaoRepositorio = new FakeAvaliacaoRepository
        {
            CriarResult = new Avaliacao
            {
                Id = 3,
                Nota = 4.5m,
                Comentario = "Excelente",
                CreatedAt = new DateTime(2026, 5, 29, 10, 0, 0, DateTimeKind.Utc)
            }
        };
        var espetaculoRepositorio = new FakeEspetaculoRepository { ExisteResult = true };
        var useCase = new CriarAvaliacaoUseCase(avaliacaoRepositorio, espetaculoRepositorio);
        var usuarioId = Guid.NewGuid();

        var resultado = await useCase.ExecutarAsync(
            7,
            usuarioId,
            new AvaliacaoCreateDto { Nota = 4.5m, Comentario = "Excelente" },
            Ct);

        Assert.Equal(3, resultado.Id);
        Assert.Equal(usuarioId, avaliacaoRepositorio.LastUsuarioId);
        Assert.Equal(7, avaliacaoRepositorio.LastEspetaculoId);
    }

    [Fact]
    public async Task CriarAvaliacao_ComNotaLimiteEComentarioNulo_RepassaDadosParaRepositorio()
    {
        var avaliacaoRepositorio = new FakeAvaliacaoRepository
        {
            CriarResult = new Avaliacao
            {
                Id = 9,
                Nota = 5,
                Comentario = null,
                CreatedAt = new DateTime(2026, 6, 12, 8, 0, 0, DateTimeKind.Utc)
            }
        };
        var espetaculoRepositorio = new FakeEspetaculoRepository { ExisteResult = true };
        var useCase = new CriarAvaliacaoUseCase(avaliacaoRepositorio, espetaculoRepositorio);
        var usuarioId = Guid.NewGuid();

        var resultado = await useCase.ExecutarAsync(
            20,
            usuarioId,
            new AvaliacaoCreateDto { Nota = 5, Comentario = null },
            Ct);

        Assert.Equal(1, espetaculoRepositorio.ExisteCalls);
        Assert.Equal(1, avaliacaoRepositorio.CriarCalls);
        Assert.Equal(20, avaliacaoRepositorio.LastEspetaculoId);
        Assert.Equal(usuarioId, avaliacaoRepositorio.LastUsuarioId);
        Assert.Equal(5, avaliacaoRepositorio.LastNota);
        Assert.Null(avaliacaoRepositorio.LastComentario);
        Assert.Equal(5, resultado.Nota);
    }

    public static TheoryData<decimal?> NotasInvalidas => new()
    {
        null,
        -0.1m,
        5.1m
    };

    [Theory]
    [MemberData(nameof(NotasInvalidas))]
    public async Task CriarAvaliacao_ComNotaInvalida_LancaValidation(decimal? nota)
    {
        var avaliacaoRepositorio = new FakeAvaliacaoRepository();
        var espetaculoRepositorio = new FakeEspetaculoRepository { ExisteResult = true };
        var useCase = new CriarAvaliacaoUseCase(avaliacaoRepositorio, espetaculoRepositorio);

        var ex = await Assert.ThrowsAsync<CatalogoValidationException>(() => useCase.ExecutarAsync(
            1,
            Guid.NewGuid(),
            new AvaliacaoCreateDto { Nota = nota },
            Ct));

        Assert.Contains("nota", ex.Details.Keys);
        Assert.Equal(0, espetaculoRepositorio.ExisteCalls);
        Assert.Equal(0, avaliacaoRepositorio.CriarCalls);
    }

    [Fact]
    public async Task CriarAvaliacao_QuandoEspetaculoNaoExiste_LancaNotFoundENaoCriaAvaliacao()
    {
        var avaliacaoRepositorio = new FakeAvaliacaoRepository();
        var espetaculoRepositorio = new FakeEspetaculoRepository { ExisteResult = false };
        var useCase = new CriarAvaliacaoUseCase(avaliacaoRepositorio, espetaculoRepositorio);

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(
            999,
            Guid.NewGuid(),
            new AvaliacaoCreateDto { Nota = 4 },
            Ct));

        Assert.Equal(1, espetaculoRepositorio.ExisteCalls);
        Assert.Equal(0, avaliacaoRepositorio.CriarCalls);
    }

    [Fact]
    public async Task ListarAvaliacoes_NormalizaPaginacaoEMapeiaUsuario()
    {
        var avaliacaoRepositorio = new FakeAvaliacaoRepository
        {
            ListarResult = ([new Avaliacao
            {
                Id = 5,
                Nota = 5,
                Comentario = "Otimo",
                CreatedAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
                Usuario = new UsuarioResumo { Id = Guid.NewGuid(), Nome = "Ana" }
            }], 201)
        };
        var useCase = new ListarAvaliacoesUseCase(
            avaliacaoRepositorio,
            new FakeEspetaculoRepository { ExisteResult = true });

        var resultado = await useCase.ExecutarAsync(7, -1, 500, Ct);

        Assert.Equal(1, avaliacaoRepositorio.LastPage);
        Assert.Equal(100, avaliacaoRepositorio.LastPageSize);
        Assert.Equal(3, resultado.Pagination.TotalPages);
        Assert.Equal("Ana", resultado.Data.Single().Usuario.Nome);
    }

    [Fact]
    public async Task ListarAvaliacoes_QuandoEspetaculoNaoExiste_LancaNotFound()
    {
        var useCase = new ListarAvaliacoesUseCase(
            new FakeAvaliacaoRepository(),
            new FakeEspetaculoRepository { ExisteResult = false });

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(88, 1, 20, Ct));
    }

    [Fact]
    public async Task ListarSessoes_QuandoEspetaculoExiste_MapeiaSessoes()
    {
        var sessaoRepositorio = new FakeSessaoRepository { Sessoes = [DetalheCompleto().Sessoes.Single()] };
        var useCase = new ListarSessoesUseCase(
            sessaoRepositorio,
            new FakeEspetaculoRepository { ExisteResult = true });

        var resultado = await useCase.ExecutarAsync(7, Ct);

        Assert.Equal("2026-06-15", resultado.Data.Single().Data);
    }

    [Fact]
    public async Task ListarCategoriasETeatros_MapeiamListasSimples()
    {
        var categorias = await new ListarCategoriasUseCase(new FakeCategoriaRepository()).ExecutarAsync(Ct);
        var teatros = await new ListarTeatrosUseCase(new FakeTeatroRepository()).ExecutarAsync(Ct);

        Assert.Equal("Musical", categorias.Data.Single().Nome);
        Assert.Equal("Teatro Alfa", teatros.Data.Single().Nome);
    }

    [Fact]
    public async Task ObterTeatro_QuandoNaoExiste_LancaNotFound()
    {
        var useCase = new ObterTeatroUseCase(new FakeTeatroRepository { Teatro = null });

        await Assert.ThrowsAsync<CatalogoNotFoundException>(() => useCase.ExecutarAsync(123, Ct));
    }

    private static EspetaculoDetalhe DetalheCompleto() => new()
    {
        Id = 7,
        Titulo = "O Auto da Compadecida",
        Descricao = "Classico brasileiro",
        Genero = "Comedia",
        Status = "em_cartaz",
        FaixaEtaria = "12+",
        ClassificacaoIndicativa = "12",
        Duracao = 120,
        Produtora = "Aventura",
        NotaMedia = 4.8m,
        Popularidade = 99,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
        Categorias = [new CategoriaResumo { Id = 1, Nome = "Comedia" }],
        Atores = [new AtorElenco { Id = 1, Nome = "Matheus", Papel = "Joao Grilo", OrdemExibicao = 1 }],
        Sessoes =
        [
            new Sessao
            {
                Id = 50,
                Data = new DateOnly(2026, 6, 15),
                Horario = new TimeOnly(20, 30),
                PrecoMinimo = 80,
                PrecoMaximo = 180,
                Disponibilidade = 42,
                LinkCompra = "https://example.com",
                Teatro = new TeatroResumo { Id = 2, Nome = "Teatro Alfa", Cidade = "Sao Paulo", Estado = "SP" }
            }
        ]
    };

    private sealed class FakeEspetaculoRepository : IEspetaculoRepository
    {
        public (IReadOnlyList<Espetaculo> Itens, int Total) ListarResult { get; set; } = ([], 0);
        public EspetaculoDetalhe? Detalhe { get; set; }
        public bool ExisteResult { get; set; }
        public int CriarResult { get; set; } = 1;
        public bool AtualizarResult { get; set; } = true;
        public bool ExcluirResult { get; set; } = true;
        public ListarEspetaculosFiltro? LastFiltro { get; private set; }
        public int LastObterDetalheId { get; private set; }
        public int CriarCalls { get; private set; }
        public int AtualizarCalls { get; private set; }
        public int ExisteCalls { get; private set; }

        public Task<(IReadOnlyList<Espetaculo> Itens, int Total)> ListarAsync(
            ListarEspetaculosFiltro filtro,
            CancellationToken cancellationToken)
        {
            LastFiltro = filtro;
            return Task.FromResult(ListarResult);
        }

        public Task<EspetaculoDetalhe?> ObterDetalheAsync(int id, CancellationToken cancellationToken)
        {
            LastObterDetalheId = id;
            return Task.FromResult(Detalhe);
        }

        public Task<bool> ExisteAsync(int id, CancellationToken cancellationToken)
        {
            ExisteCalls++;
            return Task.FromResult(ExisteResult);
        }

        public Task<int> CriarAsync(EspetaculoCreateDto dados, CancellationToken cancellationToken)
        {
            CriarCalls++;
            return Task.FromResult(CriarResult);
        }

        public Task<bool> AtualizarAsync(int id, EspetaculoUpdateDto dados, CancellationToken cancellationToken)
        {
            AtualizarCalls++;
            return Task.FromResult(AtualizarResult);
        }

        public Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken) => Task.FromResult(ExcluirResult);
    }

    private sealed class FakeAvaliacaoRepository : IAvaliacaoRepository
    {
        public (IReadOnlyList<Avaliacao> Itens, int Total) ListarResult { get; set; } = ([], 0);
        public Avaliacao CriarResult { get; set; } = new();
        public int LastPage { get; private set; }
        public int LastPageSize { get; private set; }
        public int LastEspetaculoId { get; private set; }
        public Guid LastUsuarioId { get; private set; }
        public int CriarCalls { get; private set; }
        public decimal LastNota { get; private set; }
        public string? LastComentario { get; private set; }

        public Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ListarPorEspetaculoAsync(
            int espetaculoId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            LastPage = page;
            LastPageSize = pageSize;
            return Task.FromResult(ListarResult);
        }

        public Task<Avaliacao> CriarAsync(
            int espetaculoId,
            Guid usuarioId,
            decimal nota,
            string? comentario,
            CancellationToken cancellationToken)
        {
            CriarCalls++;
            LastEspetaculoId = espetaculoId;
            LastUsuarioId = usuarioId;
            LastNota = nota;
            LastComentario = comentario;
            return Task.FromResult(CriarResult);
        }
    }

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        public Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Categoria>>(
                [new Categoria { Id = 1, Nome = "Musical", Descricao = "Espetaculos musicais" }]);
    }

    private sealed class FakeSessaoRepository : ISessaoRepository
    {
        public IReadOnlyList<Sessao> Sessoes { get; set; } = [];

        public Task<IReadOnlyList<Sessao>> ListarPorEspetaculoAsync(
            int espetaculoId,
            CancellationToken cancellationToken) => Task.FromResult(Sessoes);
    }

    private sealed class FakeTeatroRepository : ITeatroRepository
    {
        public Teatro? Teatro { get; set; } = new()
        {
            Id = 2,
            Nome = "Teatro Alfa",
            Endereco = "Rua A",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Capacidade = 500,
            Acessibilidade = true,
            ParceiroNamingRights = "Parceiro"
        };

        public Task<IReadOnlyList<Teatro>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Teatro>>(Teatro is null ? [] : [Teatro]);

        public Task<Teatro?> ObterAsync(int id, CancellationToken cancellationToken) => Task.FromResult(Teatro);
    }
}
