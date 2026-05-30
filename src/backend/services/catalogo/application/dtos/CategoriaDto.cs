namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Categoria completa (id + nome + descricao) — usada em <c>GET /catalogo/categorias</c>.</summary>
public class CategoriaDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
}

/// <summary>Categoria resumida (id + nome) — embutida em respostas de detalhe.</summary>
public class CategoriaResumoDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
}
