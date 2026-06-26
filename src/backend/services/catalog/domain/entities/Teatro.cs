namespace Spotlight.Catalogo.Domain.Entities;

/// <summary>Teatro onde ocorrem sessões (tabela <c>teatro</c>).</summary>
public class Teatro
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
