namespace Spotlight.Usuario.Infrastructure.Persistence;

public sealed record MongoTelemetryOptions(
    string ConnectionString,
    string DatabaseName,
    string CollectionName);
