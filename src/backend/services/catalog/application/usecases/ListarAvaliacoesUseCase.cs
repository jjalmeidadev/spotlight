using Spotlight.Catalogo.Application.Dtos;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Lista as avaliações de um espetáculo (paginadas). Reutiliza as constantes de
/// paginação do <see cref="ListarEspetaculosUseCase"/> para manter o limite consistente.
/// </summary>
public class ListarAvaliacoesUseCase(
    IAvaliacaoRepository avaliacaoRepositorio, IEspetaculoRepository espetaculoRepositorio)
{
    private const int PageSizePadrao = 20;
    private const int PageSizeMaximo = 100;

    public async Task<ListaPaginadaDto<AvaliacaoDto>> ExecutarAsync(
        int espetaculoId, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        if (!await espetaculoRepositorio.ExisteAsync(espetaculoId, cancellationToken))
        {
            throw new Domain.Entities.NotFoundException($"Espetáculo {espetaculoId} não encontrado.");
        }

        var paginaNorm = page is null or < 1 ? 1 : page.Value;
        var tamanhoNorm = pageSize switch
        {
            null or < 1 => PageSizePadrao,
            > PageSizeMaximo => PageSizeMaximo,
            _ => pageSize.Value
        };

        var (itens, total) = await avaliacaoRepositorio.ListarPorEspetaculoAsync(
            espetaculoId, paginaNorm, tamanhoNorm, cancellationToken);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanhoNorm);

        return new ListaPaginadaDto<AvaliacaoDto>
        {
            Data = itens.Select(a => new AvaliacaoDto
            {
                Id = a.Id,
                Nota = a.Nota,
                Comentario = a.Comentario,
                CreatedAt = a.CreatedAt,
                Usuario = new UsuarioResumoDto { Id = a.Usuario.Id, Nome = a.Usuario.Nome }
            }).ToList(),
            Pagination = new PaginacaoDto
            {
                Page = paginaNorm, PageSize = tamanhoNorm, TotalItems = total, TotalPages = totalPages
            }
        };
    }
}
