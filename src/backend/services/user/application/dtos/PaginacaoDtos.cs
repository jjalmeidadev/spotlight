namespace Spotlight.Usuario.Application.Dtos;

/// <summary>Metadados de paginação retornados junto das listas.</summary>
public class PaginacaoDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>Envelope padrão de resposta paginada: <c>{ "data": [...], "pagination": {...} }</c>.</summary>
public class ListaPaginadaDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public PaginacaoDto Pagination { get; init; } = new();
}
