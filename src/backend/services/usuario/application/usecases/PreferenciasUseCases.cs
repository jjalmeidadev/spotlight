using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

public class ObterPreferenciasUseCase(IUsuarioRepository repositorio)
{
    public async Task<PreferenciasDto> ExecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var prefs = await repositorio.ObterPreferenciasAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Perfil do usuário autenticado não encontrado.");

        return Mapear(prefs);
    }

    internal static PreferenciasDto Mapear(Preferencias p) => new()
    {
        GenerosFavoritos = [.. p.GenerosFavoritos],
        CategoriasFavoritas = [.. p.CategoriasFavoritas],
        FaixaPrecoMax = p.FaixaPrecoMax
    };
}

/// <summary>
/// Substitui as preferências do usuário (semântica de PUT — replace, não merge).
/// Valida que <c>faixaPrecoMax</c> não seja negativa.
/// </summary>
public interface IPreferenciasRecomendacaoNotifier
{
    Task NotificarPreferenciasAtualizadasAsync(
        Guid usuarioId,
        Preferencias preferencias,
        CancellationToken cancellationToken);
}

public class AtualizarPreferenciasUseCase(
    IUsuarioRepository repositorio,
    IPreferenciasRecomendacaoNotifier recomendacaoNotifier)
{
    public async Task<PreferenciasDto> ExecutarAsync(
        Guid usuarioId, PreferenciasDto dados, CancellationToken cancellationToken)
    {
        Validar(dados);

        var entidade = new Preferencias
        {
            GenerosFavoritos = dados.GenerosFavoritos ?? [],
            CategoriasFavoritas = dados.CategoriasFavoritas ?? [],
            FaixaPrecoMax = dados.FaixaPrecoMax
        };

        var atualizado = await repositorio.AtualizarPreferenciasAsync(usuarioId, entidade, cancellationToken);
        if (!atualizado) throw new NotFoundException("Perfil do usuário autenticado não encontrado.");

        await recomendacaoNotifier.NotificarPreferenciasAtualizadasAsync(
            usuarioId, entidade, cancellationToken);

        return ObterPreferenciasUseCase.Mapear(entidade);
    }

    private static void Validar(PreferenciasDto dados)
    {
        var erros = new Dictionary<string, string[]>();

        if (dados.FaixaPrecoMax is < 0)
        {
            erros["faixaPrecoMax"] = ["A faixa de preço máxima não pode ser negativa."];
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }
}
