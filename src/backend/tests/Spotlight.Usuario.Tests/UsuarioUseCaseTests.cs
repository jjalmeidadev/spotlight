using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;
using Xunit;

namespace Spotlight.Usuario.Tests;

public class ObterUsuarioMeUseCaseTests
{
    [Fact]
    public async Task NotFound_Quando_Perfil_Inexiste()
    {
        var useCase = new ObterUsuarioMeUseCase(new UsuarioRepoFake { PerfilResultado = null });

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Mapeia_Campos_Para_Dto()
    {
        var id = Guid.NewGuid();
        var repo = new UsuarioRepoFake
        {
            PerfilResultado = new Spotlight.Usuario.Domain.Entities.Usuario
            {
                Id = id, Nome = "Maria", Email = "m@x.com",
                DataNascimento = new DateOnly(1995, 4, 12), Localizacao = "Campinas, SP",
                TipoUsuario = "cadastrado", DataCadastro = new DateTime(2026, 3, 1)
            }
        };
        var useCase = new ObterUsuarioMeUseCase(repo);

        var dto = await useCase.ExecutarAsync(id, CancellationToken.None);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Maria", dto.Nome);
        Assert.Equal("cadastrado", dto.TipoUsuario);
        Assert.Equal(new DateOnly(1995, 4, 12), dto.DataNascimento);
    }
}

public class AtualizarUsuarioMeUseCaseTests
{
    [Fact]
    public async Task Rejeita_Nome_Vazio()
    {
        var useCase = new AtualizarUsuarioMeUseCase(new UsuarioRepoFake());
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(),
                new UsuarioUpdateDto { Nome = "   " }, CancellationToken.None));
        Assert.True(ex.Details.ContainsKey("nome"));
    }

    [Fact]
    public async Task Rejeita_Nome_Muito_Longo()
    {
        var useCase = new AtualizarUsuarioMeUseCase(new UsuarioRepoFake());
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(),
                new UsuarioUpdateDto { Nome = new string('a', 151) }, CancellationToken.None));
        Assert.True(ex.Details.ContainsKey("nome"));
    }

    [Fact]
    public async Task NotFound_Quando_Id_Inexiste()
    {
        var useCase = new AtualizarUsuarioMeUseCase(
            new UsuarioRepoFake { AtualizarResultado = false });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(),
                new UsuarioUpdateDto { Nome = "Novo" }, CancellationToken.None));
    }

    [Fact]
    public async Task Aplica_Update_E_Retorna_Perfil_Atualizado()
    {
        var id = Guid.NewGuid();
        var repo = new UsuarioRepoFake
        {
            AtualizarResultado = true,
            PerfilResultado = new Spotlight.Usuario.Domain.Entities.Usuario
                { Id = id, Nome = "Novo Nome", Email = "m@x.com", TipoUsuario = "cadastrado" }
        };
        var useCase = new AtualizarUsuarioMeUseCase(repo);

        var dto = await useCase.ExecutarAsync(id,
            new UsuarioUpdateDto { Nome = "Novo Nome" }, CancellationToken.None);

        Assert.Equal("Novo Nome", dto.Nome);
        Assert.Equal(id, repo.AtualizarRecebido?.Id);
    }
}

public class PreferenciasUseCaseTests
{
    [Fact]
    public async Task Obter_NotFound_Quando_Usuario_Inexiste()
    {
        var useCase = new ObterPreferenciasUseCase(
            new UsuarioRepoFake { PreferenciasResultado = null });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Obter_Mapeia_Listas()
    {
        var useCase = new ObterPreferenciasUseCase(new UsuarioRepoFake
        {
            PreferenciasResultado = new Preferencias
            {
                GenerosFavoritos = ["Comédia", "Musical"],
                CategoriasFavoritas = [3, 7],
                FaixaPrecoMax = 150m
            }
        });

        var dto = await useCase.ExecutarAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(["Comédia", "Musical"], dto.GenerosFavoritos);
        Assert.Equal([3, 7], dto.CategoriasFavoritas);
        Assert.Equal(150m, dto.FaixaPrecoMax);
    }

    [Fact]
    public async Task Atualizar_Rejeita_FaixaPrecoMax_Negativa()
    {
        var useCase = new AtualizarPreferenciasUseCase(new UsuarioRepoFake());
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(),
                new PreferenciasDto { FaixaPrecoMax = -1m }, CancellationToken.None));
        Assert.True(ex.Details.ContainsKey("faixaPrecoMax"));
    }

    [Fact]
    public async Task Atualizar_NotFound_Quando_Usuario_Inexiste()
    {
        var useCase = new AtualizarPreferenciasUseCase(
            new UsuarioRepoFake { AtualizarPreferenciasResultado = false });
        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(),
                new PreferenciasDto(), CancellationToken.None));
    }
}

public class ListarAvaliacoesUsuarioUseCaseTests
{
    [Fact]
    public async Task Normaliza_Page_E_PageSize()
    {
        var repo = new AvaliacaoUsuarioRepoFake();
        var useCase = new ListarAvaliacoesUsuarioUseCase(repo);

        await useCase.ExecutarAsync(Guid.NewGuid(), page: 0, pageSize: 500, CancellationToken.None);

        Assert.NotNull(repo.Recebido);
        Assert.Equal(1, repo.Recebido!.Value.Page);
        Assert.Equal(100, repo.Recebido.Value.PageSize); // teto = 100
    }

    [Fact]
    public async Task Calcula_TotalPages_Arredondando_Para_Cima()
    {
        var repo = new AvaliacaoUsuarioRepoFake { Total = 8 };
        var useCase = new ListarAvaliacoesUsuarioUseCase(repo);

        var resultado = await useCase.ExecutarAsync(
            Guid.NewGuid(), page: 1, pageSize: 5, CancellationToken.None);

        Assert.Equal(8, resultado.Pagination.TotalItems);
        Assert.Equal(2, resultado.Pagination.TotalPages); // ceil(8/5)=2
    }

    [Fact]
    public async Task TotalPages_Zero_Quando_Sem_Resultados()
    {
        var useCase = new ListarAvaliacoesUsuarioUseCase(new AvaliacaoUsuarioRepoFake());
        var resultado = await useCase.ExecutarAsync(
            Guid.NewGuid(), null, null, CancellationToken.None);
        Assert.Equal(0, resultado.Pagination.TotalPages);
        Assert.Empty(resultado.Data);
    }

    [Fact]
    public async Task Mapeia_Itens_Com_Espetaculo_Embedado()
    {
        var repo = new AvaliacaoUsuarioRepoFake
        {
            Total = 1,
            Itens = [new AvaliacaoUsuario
            {
                Id = 41, Nota = 4.5m, Comentario = "Top",
                CreatedAt = new DateTime(2026, 5, 18),
                Espetaculo = new EspetaculoResumoUsuario { Id = 12, Titulo = "O Auto" }
            }]
        };
        var useCase = new ListarAvaliacoesUsuarioUseCase(repo);

        var resultado = await useCase.ExecutarAsync(
            Guid.NewGuid(), null, null, CancellationToken.None);

        var item = Assert.Single(resultado.Data);
        Assert.Equal(41, item.Id);
        Assert.Equal(12, item.Espetaculo.Id);
        Assert.Equal("O Auto", item.Espetaculo.Titulo);
    }
}
