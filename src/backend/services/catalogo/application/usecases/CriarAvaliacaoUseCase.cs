using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Cria uma avaliação de espetáculo. Valida a nota (1–5), confere se o espetáculo
/// existe (404) e deixa o repositório lidar com a violação de UNIQUE (409).
/// </summary>
public class CriarAvaliacaoUseCase(
    IAvaliacaoRepository avaliacaoRepositorio,
    IEspetaculoRepository espetaculoRepositorio,
    IAvaliacaoRecomendacaoClient recomendacaoClient)
{
    public async Task<AvaliacaoCriadaDto> ExecutarAsync(
        int espetaculoId, Guid usuarioId, AvaliacaoCreateDto dados, CancellationToken cancellationToken)
    {
        Validar(dados);

        if (!await espetaculoRepositorio.ExisteAsync(espetaculoId, cancellationToken))
        {
            throw new NotFoundException($"Espetáculo {espetaculoId} não encontrado.");
        }

        var avaliacao = await avaliacaoRepositorio.CriarAsync(
            espetaculoId, usuarioId, dados.Nota!.Value, dados.Comentario, cancellationToken);

        await recomendacaoClient.NotificarAvaliacaoCriadaAsync(
            new AvaliacaoCriadaEvento(
                avaliacao.Id,
                espetaculoId,
                usuarioId,
                avaliacao.Nota,
                avaliacao.Comentario,
                avaliacao.CreatedAt),
            cancellationToken);

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
            erros["nota"] = ["A nota é obrigatória."];
        }
        else if (dados.Nota < 1 || dados.Nota > 5)
        {
            erros["nota"] = ["A nota deve estar entre 1 e 5."];
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }
}
