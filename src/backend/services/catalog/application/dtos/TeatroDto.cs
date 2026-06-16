namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Teatro completo — usado em <c>GET /catalogo/teatros[/{id}]</c>.</summary>
public class TeatroDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Endereco { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public int? Capacidade { get; init; }
    public bool Acessibilidade { get; init; }
    public string? ParceiroNamingRights { get; init; }
}

/// <summary>Teatro resumido — embutido em sessões.</summary>
public class TeatroResumoDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}
