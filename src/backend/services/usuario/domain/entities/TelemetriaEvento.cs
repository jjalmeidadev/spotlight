using System.Text.Json;

namespace Spotlight.Usuario.Domain.Entities;

public sealed record TelemetriaEntidade(string Type, string Id);

public sealed record TelemetriaEvento
{
    public required string EventId { get; init; }
    public Guid? UserId { get; init; }
    public string? AnonymousId { get; init; }
    public required string SessionId { get; init; }
    public required string EventType { get; init; }
    public required string Source { get; init; }
    public string? Screen { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public TelemetriaEntidade? Entity { get; init; }
    public JsonElement? Properties { get; init; }
    public JsonElement? Device { get; init; }
}
