namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>
/// Filtros e paginação aceitos por <c>GET /catalogo/espetaculos</c>.
/// Os valores são vinculados a partir da query string e normalizados no use case.
/// </summary>
public class ListarEspetaculosFiltro
{
    /// <summary>Busca por título (correspondência parcial, case-insensitive).</summary>
    public string? Q { get; set; }

    /// <summary>Filtro por status: <c>em_cartaz</c>, <c>em_breve</c> ou <c>encerrado</c>.</summary>
    public string? Status { get; set; }

    public string? Genero { get; set; }
    public int? CategoriaId { get; set; }

    /// <summary>Ordenação: <c>popularidade</c> (default), <c>nota_media</c> ou <c>titulo</c>.</summary>
    public string? OrdenarPor { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
