using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Infrastructure.Persistence;

public sealed class MongoTelemetriaRepository(
    IMongoDatabase database,
    MongoTelemetryOptions options) : ITelemetriaRepository
{
    private readonly IMongoCollection<BsonDocument> collection =
        database.GetCollection<BsonDocument>(options.CollectionName);

    public Task InserirAsync(TelemetriaEvento evento, CancellationToken cancellationToken)
    {
        var document = new BsonDocument
        {
            ["event_id"] = evento.EventId,
            ["event_type"] = evento.EventType,
            ["source"] = evento.Source,
            ["session_id"] = evento.SessionId,
            ["occurred_at"] = evento.OccurredAt.UtcDateTime,
            ["received_at"] = evento.ReceivedAt.UtcDateTime
        };

        if (evento.UserId is not null) document["user_id"] = evento.UserId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(evento.AnonymousId)) document["anonymous_id"] = evento.AnonymousId;
        if (!string.IsNullOrWhiteSpace(evento.Screen)) document["screen"] = evento.Screen;

        if (evento.Entity is not null)
        {
            document["entity"] = new BsonDocument
            {
                ["type"] = evento.Entity.Type,
                ["id"] = evento.Entity.Id
            };
        }

        if (evento.Properties is not null) document["properties"] = ToBsonValue(evento.Properties.Value);
        if (evento.Device is not null) document["device"] = ToBsonValue(evento.Device.Value);

        return collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    private static BsonValue ToBsonValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => ToBsonDocument(element),
        JsonValueKind.Array => new BsonArray(element.EnumerateArray().Select(ToBsonValue)),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
        JsonValueKind.Number when element.TryGetDouble(out var doubleValue) => doubleValue,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => BsonNull.Value,
        JsonValueKind.Undefined => BsonNull.Value,
        _ => BsonNull.Value
    };

    private static BsonDocument ToBsonDocument(JsonElement element)
    {
        var document = new BsonDocument();
        foreach (var property in element.EnumerateObject())
        {
            document[property.Name] = ToBsonValue(property.Value);
        }

        return document;
    }
}
