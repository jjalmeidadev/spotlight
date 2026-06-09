using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

public interface ITelemetriaRepository
{
    Task InserirAsync(TelemetriaEvento evento, CancellationToken cancellationToken);
}

public interface ITelemetriaService
{
    Task<string> RegistrarAsync(
        Guid? usuarioId,
        RegistrarTelemetriaRequest request,
        CancellationToken cancellationToken);
}

public sealed class TelemetriaService(ITelemetriaRepository repository) : ITelemetriaService
{
    public async Task<string> RegistrarAsync(
        Guid? usuarioId,
        RegistrarTelemetriaRequest request,
        CancellationToken cancellationToken)
    {
        Validar(usuarioId, request);

        var eventId = string.IsNullOrWhiteSpace(request.EventId)
            ? Guid.NewGuid().ToString("N")
            : request.EventId.Trim();

        var evento = new TelemetriaEvento
        {
            EventId = eventId,
            UserId = usuarioId,
            AnonymousId = NormalizarOpcional(request.AnonymousId),
            SessionId = request.SessionId.Trim(),
            EventType = request.EventType.Trim(),
            Source = request.Source.Trim(),
            Screen = NormalizarOpcional(request.Screen),
            OccurredAt = (request.OccurredAt ?? DateTimeOffset.UtcNow).ToUniversalTime(),
            ReceivedAt = DateTimeOffset.UtcNow,
            Entity = request.Entity is null
                ? null
                : new TelemetriaEntidade(request.Entity.Type.Trim(), request.Entity.Id.Trim()),
            Properties = request.Properties,
            Device = request.Device
        };

        await repository.InserirAsync(evento, cancellationToken);
        return evento.EventId;
    }

    private static void Validar(Guid? usuarioId, RegistrarTelemetriaRequest request)
    {
        var erros = new Dictionary<string, string[]>();

        if (usuarioId is null && string.IsNullOrWhiteSpace(request.AnonymousId))
        {
            erros["usuario"] = ["Informe um usuario autenticado ou anonymousId."];
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            erros["eventType"] = ["Tipo do evento e obrigatorio."];
        }

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            erros["source"] = ["Origem do evento e obrigatoria."];
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            erros["sessionId"] = ["Sessao do evento e obrigatoria."];
        }

        if (request.Entity is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Entity.Type))
            {
                erros["entity.type"] = ["Tipo da entidade e obrigatorio quando entity for informada."];
            }

            if (string.IsNullOrWhiteSpace(request.Entity.Id))
            {
                erros["entity.id"] = ["Identificador da entidade e obrigatorio quando entity for informada."];
            }
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
