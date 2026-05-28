namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>
/// Envelope simples de lista (sem paginação): <c>{ "data": [...] }</c>.
/// Usado por <c>GET /catalogo/categorias</c>, <c>GET /catalogo/teatros</c> e
/// <c>GET /catalogo/.../sessoes</c>.
/// </summary>
public class ListaDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
}
