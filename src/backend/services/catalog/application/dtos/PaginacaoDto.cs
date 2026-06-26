namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Metadados de paginação retornados junto das listas.</summary>
public class PaginacaoDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}
