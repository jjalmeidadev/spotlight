using MongoDB.Bson;
using MongoDB.Driver;

namespace Spotlight.Usuario.Infrastructure.Persistence;

public sealed record MongoTelemetryIndexDefinition(
    string Name,
    BsonDocument Keys,
    bool Unique = false,
    bool Sparse = false);

public static class MongoTelemetryIndexes
{
    public static IReadOnlyList<MongoTelemetryIndexDefinition> Definitions { get; } =
    [
        new(
            Name: "ux_event_id",
            Keys: new BsonDocument("event_id", 1),
            Unique: true,
            Sparse: true),
        new(
            Name: "ix_user_time",
            Keys: new BsonDocument
            {
                ["user_id"] = 1,
                ["occurred_at"] = -1
            }),
        new(
            Name: "ix_anonymous_time",
            Keys: new BsonDocument
            {
                ["anonymous_id"] = 1,
                ["occurred_at"] = -1
            }),
        new(
            Name: "ix_entity_time",
            Keys: new BsonDocument
            {
                ["entity.type"] = 1,
                ["entity.id"] = 1,
                ["occurred_at"] = -1
            }),
        new(
            Name: "ix_event_type_time",
            Keys: new BsonDocument
            {
                ["event_type"] = 1,
                ["occurred_at"] = -1
            }),
        new(
            Name: "ix_session_time",
            Keys: new BsonDocument
            {
                ["session_id"] = 1,
                ["occurred_at"] = 1
            })
    ];

    public static IReadOnlyList<CreateIndexModel<BsonDocument>> CreateModels() =>
        Definitions
            .Select(definition => new CreateIndexModel<BsonDocument>(
                definition.Keys,
                new CreateIndexOptions
                {
                    Name = definition.Name,
                    Unique = definition.Unique,
                    Sparse = definition.Sparse
                }))
            .ToArray();
}

public sealed class MongoTelemetryIndexInitializer(
    MongoTelemetryOptions options,
    ILogger<MongoTelemetryIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DatabaseName);
            var collection = database.GetCollection<BsonDocument>(options.CollectionName);

            await collection.Indexes.CreateManyAsync(
                MongoTelemetryIndexes.CreateModels(),
                cancellationToken);

            logger.LogInformation(
                "Indices MongoDB de telemetria garantidos na collection {CollectionName}.",
                options.CollectionName);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Nao foi possivel garantir os indices MongoDB de telemetria no startup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
