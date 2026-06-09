using System.Text.Json;

namespace Spotlight.Usuario.Application.Dtos;

public sealed record TelemetriaEntityDto(string Type, string Id);

public sealed record RegistrarTelemetriaRequest(
    string? EventId,
    string EventType,
    string Source,
    string SessionId,
    DateTimeOffset? OccurredAt,
    string? Screen,
    TelemetriaEntityDto? Entity,
    JsonElement? Properties,
    JsonElement? Device,
    string? AnonymousId);
