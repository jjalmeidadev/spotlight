using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Helpers de validação reutilizados por criação e atualização de espetáculo.
/// Acumula erros num único <see cref="ValidationException"/> (envelope com details).
/// </summary>
internal static class Validacoes
{
    private static readonly HashSet<string> StatusValidos = ["em_cartaz", "em_breve", "encerrado"];
    private const int TamanhoMaximoTitulo = 200;

    public static void ValidarEspetaculo(
        string? titulo, int? duracao, string? status,
        IReadOnlyList<AtorInputDto>? atores, bool obrigaTitulo)
    {
        var erros = new Dictionary<string, string[]>();

        if (obrigaTitulo && string.IsNullOrWhiteSpace(titulo))
        {
            erros["titulo"] = ["O título é obrigatório."];
        }
        else if (titulo is { Length: > TamanhoMaximoTitulo })
        {
            erros["titulo"] = [$"O título deve ter no máximo {TamanhoMaximoTitulo} caracteres."];
        }

        if (duracao is <= 0)
        {
            erros["duracao"] = ["A duração deve ser maior que zero."];
        }

        if (status is not null && !StatusValidos.Contains(status))
        {
            erros["status"] = ["Status inválido. Use: em_cartaz, em_breve ou encerrado."];
        }

        if (atores is not null && atores.Any(a => a.AtorId is null))
        {
            erros["atores"] = ["Cada ator do elenco precisa de atorId."];
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }
}
