namespace Spotlight.Catalogo.Application.UseCases;

public interface IAvaliacaoRecomendacaoClient
{
    Task NotificarAvaliacaoCriadaAsync(
        AvaliacaoCriadaEvento evento,
        CancellationToken cancellationToken);
}

public sealed record AvaliacaoCriadaEvento(
    int AvaliacaoId,
    int EspetaculoId,
    Guid UsuarioId,
    decimal Nota,
    string? Comentario,
    DateTime CreatedAt);
