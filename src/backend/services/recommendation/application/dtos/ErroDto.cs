namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>
/// Envelope padrão de erro (alinhado ao contrato de API). O campo
/// <see cref="Details"/> é populado apenas em erros de validação (400).
/// </summary>
public class ErroDto
{
    /// <summary>Mensagem curta e legível descrevendo a falha.</summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Identificador do trace para correlação com os logs do servidor.</summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>Detalhes por campo em erros de validação (chave → lista de mensagens). <c>null</c> fora do 400.</summary>
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}
