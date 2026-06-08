using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Spotlight.Usuario.Infrastructure.Persistence;

public sealed class MongoDbHealthCheck(
    IMongoClient mongoClient,
    MongoTelemetryOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = mongoClient.GetDatabase(options.DatabaseName);
            await database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy(
                $"MongoDB database '{options.DatabaseName}' acessivel.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Falha ao conectar no MongoDB de telemetria.",
                exception);
        }
    }
}
