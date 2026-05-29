namespace Spotlight.Usuario.Domain.Entities;

/// <summary>Lançada quando um recurso solicitado não existe → middleware traduz para 404.</summary>
public class NotFoundException(string mensagem) : Exception(mensagem);

/// <summary>
/// Lançada em falhas de validação de entrada → middleware traduz para 400 com
/// o campo <c>details</c> (chave = nome do campo, valor = lista de mensagens).
/// </summary>
public class ValidationException(IReadOnlyDictionary<string, string[]> details, string? mensagem = null)
    : Exception(mensagem ?? "Dados de entrada inválidos.")
{
    public IReadOnlyDictionary<string, string[]> Details { get; } = details;
}

/// <summary>Lançada em violação de invariantes/uniqueness → middleware traduz para 409.</summary>
public class ConflictException(string mensagem) : Exception(mensagem);
