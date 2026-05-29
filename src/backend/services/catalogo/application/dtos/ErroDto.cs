namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>
/// Envelope padrão de erro (alinhado ao contrato de API). O campo
/// <see cref="Details"/> é populado apenas em erros de validação (400).
/// </summary>
public class ErroDto
{
    public string Error { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}
