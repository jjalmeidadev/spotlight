using Spotlight.Usuario.Application.Dtos;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Lista as avaliações que o usuário autenticado já fez (paginadas).
/// Não precisa checar existência do usuário — o JWT validado já garante isso.
/// </summary>
public class ListarAvaliacoesUsuarioUseCase(IAvaliacaoUsuarioRepository repositorio)
{
    private const int PageSizePadrao = 20;
    private const int PageSizeMaximo = 100;

    public async Task<ListaPaginadaDto<AvaliacaoUsuarioDto>> ExecutarAsync(
        Guid usuarioId, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var paginaNorm = page is null or < 1 ? 1 : page.Value;
        var tamanhoNorm = pageSize switch
        {
            null or < 1 => PageSizePadrao,
            > PageSizeMaximo => PageSizeMaximo,
            _ => pageSize.Value
        };

        var (itens, total) = await repositorio.ListarPorUsuarioAsync(
            usuarioId, paginaNorm, tamanhoNorm, cancellationToken);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanhoNorm);

        return new ListaPaginadaDto<AvaliacaoUsuarioDto>
        {
            Data = itens.Select(a => new AvaliacaoUsuarioDto
            {
                Id = a.Id,
                Nota = a.Nota,
                Comentario = a.Comentario,
                CreatedAt = a.CreatedAt,
                Espetaculo = new EspetaculoResumoUsuarioDto { Id = a.Espetaculo.Id, Titulo = a.Espetaculo.Titulo }
            }).ToList(),
            Pagination = new PaginacaoDto
            {
                Page = paginaNorm, PageSize = tamanhoNorm, TotalItems = total, TotalPages = totalPages
            }
        };
    }
}
