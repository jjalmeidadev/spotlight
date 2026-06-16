namespace Spotlight.Geolocalizacao.Domain.Entities;

public class NotFoundException(string mensagem) : Exception(mensagem);

public class ValidationException(IReadOnlyDictionary<string, string[]> details, string? mensagem = null)
    : Exception(mensagem ?? "Dados de entrada invalidos.")
{
    public IReadOnlyDictionary<string, string[]> Details { get; } = details;
}

public class ConflictException(string mensagem) : Exception(mensagem);
