using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Salva uma avaliacao de espetaculo. Valida a nota (0-5), confere se o espetaculo
/// existe (404) e atualiza a avaliacao anterior quando o usuario ja avaliou.
/// </summary>
public class CriarAvaliacaoUseCase(
    IAvaliacaoRepository avaliacaoRepositorio, IEspetaculoRepository espetaculoRepositorio)
{
    public async Task<AvaliacaoCriadaDto> ExecutarAsync(
        int espetaculoId, Guid usuarioId, AvaliacaoCreateDto dados, CancellationToken cancellationToken)
    {
        Validar(dados);

        if (!await espetaculoRepositorio.ExisteAsync(espetaculoId, cancellationToken))
        {
            throw new NotFoundException($"Espetaculo {espetaculoId} nao encontrado.");
        }

        var avaliacao = await avaliacaoRepositorio.CriarAsync(
            espetaculoId, usuarioId, dados.Nota!.Value, dados.Comentario, cancellationToken);

        return new AvaliacaoCriadaDto
        {
            Id = avaliacao.Id,
            Nota = avaliacao.Nota,
            Comentario = avaliacao.Comentario,
            CreatedAt = avaliacao.CreatedAt
        };
    }

    private static void Validar(AvaliacaoCreateDto dados)
    {
        var erros = new Dictionary<string, string[]>();

        if (dados.Nota is null)
        {
            erros["nota"] = ["A nota e obrigatoria."];
        }
        else if (dados.Nota < 0 || dados.Nota > 5)
        {
            erros["nota"] = ["A nota deve estar entre 0 e 5."];
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }
}
