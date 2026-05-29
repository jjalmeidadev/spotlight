namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>
/// Envelope padrão de resposta paginada: <c>{ "data": [...], "pagination": {...} }</c>.
/// </summary>
public class ListaPaginadaDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public PaginacaoDto Pagination { get; init; } = new();
}
