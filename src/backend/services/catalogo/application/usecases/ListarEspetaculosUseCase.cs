using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Caso de uso da listagem de espetáculos: normaliza os parâmetros de entrada,
/// delega a consulta ao repositório e monta o envelope paginado de saída.
/// </summary>
public class ListarEspetaculosUseCase(IEspetaculoRepository repositorio)
{
    private const int PageSizePadrao = 20;
    private const int PageSizeMaximo = 100;
    private const string OrdenacaoPadrao = "popularidade";

    private static readonly HashSet<string> OrdenacoesValidas =
        ["popularidade", "nota_media", "titulo"];

    public async Task<ListaPaginadaDto<EspetaculoResumoDto>> ExecutarAsync(
        ListarEspetaculosFiltro filtro, CancellationToken cancellationToken)
    {
        Normalizar(filtro);

        var (itens, total) = await repositorio.ListarAsync(filtro, cancellationToken);

        var totalPages = total == 0
            ? 0
            : (int)Math.Ceiling(total / (double)filtro.PageSize);

        return new ListaPaginadaDto<EspetaculoResumoDto>
        {
            Data = itens.Select(Mapear).ToList(),
            Pagination = new PaginacaoDto
            {
                Page = filtro.Page,
                PageSize = filtro.PageSize,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }

    /// <summary>Garante page/pageSize dentro dos limites e ordenação válida.</summary>
    private static void Normalizar(ListarEspetaculosFiltro filtro)
    {
        if (filtro.Page < 1) filtro.Page = 1;

        if (filtro.PageSize < 1) filtro.PageSize = PageSizePadrao;
        else if (filtro.PageSize > PageSizeMaximo) filtro.PageSize = PageSizeMaximo;

        filtro.OrdenarPor = filtro.OrdenarPor is not null && OrdenacoesValidas.Contains(filtro.OrdenarPor)
            ? filtro.OrdenarPor
            : OrdenacaoPadrao;
    }

    private static EspetaculoResumoDto Mapear(Espetaculo e) => new()
    {
        Id = e.Id,
        Titulo = e.Titulo,
        Genero = e.Genero,
        Status = e.Status,
        FaixaEtaria = e.FaixaEtaria,
        ClassificacaoIndicativa = e.ClassificacaoIndicativa,
        Duracao = e.Duracao,
        NotaMedia = e.NotaMedia,
        Popularidade = e.Popularidade,
        Midias = e.Midias.Select(MapearMidia).ToList()
    };

    internal static EspetaculoMidiaDto MapearMidia(EspetaculoMidia midia) => new()
    {
        TipoMidia = midia.TipoMidia,
        StorageBucket = midia.StorageBucket,
        StoragePath = midia.StoragePath,
        AltText = midia.AltText,
        OrdemExibicao = midia.OrdemExibicao
    };
}
