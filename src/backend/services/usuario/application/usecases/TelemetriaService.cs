using System.Text;
using System.Text.Json;
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
    private const int MaxPayloadMetadataBytes = 16 * 1024;
    private const int MaxEventIdLength = 128;
    private const int MaxAnonymousIdLength = 128;
    private const int MaxSessionIdLength = 128;
    private const int MaxScreenLength = 100;
    private const int MaxEntityIdLength = 128;
    private static readonly TimeSpan MaxFutureTolerance = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxEventAge = TimeSpan.FromDays(180);
    private static readonly HashSet<string> EventTypes =
    [
        "session_started",
        "screen_viewed",
        "show_card_clicked",
        "show_viewed",
        "search_performed",
        "favorite_toggled",
        "purchase_clicked",
        "session_ended"
    ];
    private static readonly HashSet<string> EntityTypes =
    [
        "espetaculo",
        "sessao",
        "teatro",
        "categoria",
        "busca"
    ];

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

        if (!string.IsNullOrWhiteSpace(request.EventId) && request.EventId.Trim().Length > MaxEventIdLength)
        {
            erros["eventId"] = [$"Identificador do evento deve ter no maximo {MaxEventIdLength} caracteres."];
        }

        if (!string.IsNullOrWhiteSpace(request.AnonymousId) && request.AnonymousId.Trim().Length > MaxAnonymousIdLength)
        {
            erros["anonymousId"] = [$"Identificador anonimo deve ter no maximo {MaxAnonymousIdLength} caracteres."];
        }

        var eventType = request.EventType?.Trim() ?? string.Empty;
        var source = request.Source?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(eventType))
        {
            erros["eventType"] = ["Tipo do evento e obrigatorio."];
        }
        else if (!EventTypes.Contains(eventType))
        {
            erros["eventType"] = ["Tipo do evento nao pertence ao catalogo de telemetria."];
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            erros["source"] = ["Origem do evento e obrigatoria."];
        }
        else if (source != "mobile")
        {
            erros["source"] = ["Origem do evento deve ser mobile."];
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            erros["sessionId"] = ["Sessao do evento e obrigatoria."];
        }
        else if (request.SessionId.Trim().Length > MaxSessionIdLength)
        {
            erros["sessionId"] = [$"Sessao do evento deve ter no maximo {MaxSessionIdLength} caracteres."];
        }

        if (!string.IsNullOrWhiteSpace(request.Screen) && request.Screen.Trim().Length > MaxScreenLength)
        {
            erros["screen"] = [$"Nome da tela deve ter no maximo {MaxScreenLength} caracteres."];
        }

        ValidarData(request.OccurredAt, erros);
        ValidarMetadados(request.Properties, request.Device, erros);

        if (request.Entity is not null)
        {
            var entityType = request.Entity.Type?.Trim() ?? string.Empty;
            var entityId = request.Entity.Id?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(entityType))
            {
                erros["entity.type"] = ["Tipo da entidade e obrigatorio quando entity for informada."];
            }
            else if (!EntityTypes.Contains(entityType))
            {
                erros["entity.type"] = ["Tipo da entidade nao pertence ao catalogo de telemetria."];
            }

            if (string.IsNullOrWhiteSpace(entityId))
            {
                erros["entity.id"] = ["Identificador da entidade e obrigatorio quando entity for informada."];
            }
            else if (entityId.Length > MaxEntityIdLength)
            {
                erros["entity.id"] = [$"Identificador da entidade deve ter no maximo {MaxEntityIdLength} caracteres."];
            }
        }

        ValidarRegrasPorEvento(eventType, request, erros);

        if (erros.Count > 0) throw new ValidationException(erros);
    }

    private static void ValidarData(
        DateTimeOffset? occurredAt,
        IDictionary<string, string[]> erros)
    {
        if (occurredAt is null) return;

        var agora = DateTimeOffset.UtcNow;
        var dataEvento = occurredAt.Value.ToUniversalTime();

        if (dataEvento > agora.Add(MaxFutureTolerance))
        {
            erros["occurredAt"] = ["Data do evento nao pode estar mais de 5 minutos no futuro."];
            return;
        }

        if (dataEvento < agora.Subtract(MaxEventAge))
        {
            erros["occurredAt"] = ["Data do evento nao pode ser anterior a 180 dias."];
        }
    }

    private static void ValidarMetadados(
        JsonElement? properties,
        JsonElement? device,
        IDictionary<string, string[]> erros)
    {
        var propertiesBytes = ValidarObjetoJson(properties, "properties", erros);
        var deviceBytes = ValidarObjetoJson(device, "device", erros);

        if (propertiesBytes + deviceBytes > MaxPayloadMetadataBytes)
        {
            erros["metadata"] = [$"properties e device devem somar no maximo {MaxPayloadMetadataBytes} bytes."];
        }
    }

    private static int ValidarObjetoJson(
        JsonElement? valor,
        string campo,
        IDictionary<string, string[]> erros)
    {
        if (valor is null) return 0;

        if (valor.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return 0;
        }

        if (valor.Value.ValueKind != JsonValueKind.Object)
        {
            erros[campo] = [$"{campo} deve ser um objeto JSON."];
            return 0;
        }

        return Encoding.UTF8.GetByteCount(valor.Value.GetRawText());
    }

    private static void ValidarRegrasPorEvento(
        string eventType,
        RegistrarTelemetriaRequest request,
        IDictionary<string, string[]> erros)
    {
        switch (eventType)
        {
            case "screen_viewed":
                ExigirPropriedadeString(request.Properties, "routeName", erros);
                break;
            case "show_card_clicked":
                ExigirEntidade(request.Entity, "espetaculo", erros);
                ExigirPropriedadeString(request.Properties, "origin", erros);
                break;
            case "show_viewed":
                ExigirEntidade(request.Entity, "espetaculo", erros);
                break;
            case "search_performed":
                ExigirEntidade(request.Entity, "busca", erros);
                ExigirPropriedadeString(request.Properties, "query", erros);
                ExigirPropriedadeNumero(request.Properties, "resultsCount", erros);
                break;
            case "favorite_toggled":
                ExigirEntidade(request.Entity, "espetaculo", erros);
                ExigirPropriedadeBooleana(request.Properties, "active", erros);
                break;
            case "purchase_clicked":
                ExigirEntidade(request.Entity, ["sessao", "espetaculo"], erros);
                break;
            case "session_ended":
                ExigirPropriedadeNumero(request.Properties, "durationMs", erros);
                break;
        }
    }

    private static void ExigirEntidade(
        TelemetriaEntityDto? entity,
        string tipoEsperado,
        IDictionary<string, string[]> erros) =>
        ExigirEntidade(entity, [tipoEsperado], erros);

    private static void ExigirEntidade(
        TelemetriaEntityDto? entity,
        IReadOnlyCollection<string> tiposEsperados,
        IDictionary<string, string[]> erros)
    {
        if (entity is null)
        {
            erros["entity"] = ["Entidade e obrigatoria para este tipo de evento."];
            return;
        }

        var entityType = entity.Type?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(entityType) && !tiposEsperados.Contains(entityType))
        {
            erros["entity.type"] = ["Tipo da entidade nao e compativel com este tipo de evento."];
        }
    }

    private static void ExigirPropriedadeString(
        JsonElement? properties,
        string nome,
        IDictionary<string, string[]> erros)
    {
        if (!TryGetProperty(properties, nome, out var valor) ||
            valor.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(valor.GetString()))
        {
            erros[$"properties.{nome}"] = [$"Propriedade {nome} e obrigatoria para este tipo de evento."];
        }
    }

    private static void ExigirPropriedadeNumero(
        JsonElement? properties,
        string nome,
        IDictionary<string, string[]> erros)
    {
        if (!TryGetProperty(properties, nome, out var valor) ||
            valor.ValueKind is not (JsonValueKind.Number))
        {
            erros[$"properties.{nome}"] = [$"Propriedade {nome} numerica e obrigatoria para este tipo de evento."];
        }
    }

    private static void ExigirPropriedadeBooleana(
        JsonElement? properties,
        string nome,
        IDictionary<string, string[]> erros)
    {
        if (!TryGetProperty(properties, nome, out var valor) ||
            valor.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            erros[$"properties.{nome}"] = [$"Propriedade {nome} booleana e obrigatoria para este tipo de evento."];
        }
    }

    private static bool TryGetProperty(
        JsonElement? properties,
        string nome,
        out JsonElement valor)
    {
        valor = default;
        return properties is not null &&
            properties.Value.ValueKind == JsonValueKind.Object &&
            properties.Value.TryGetProperty(nome, out valor);
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
