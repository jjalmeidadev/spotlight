using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace Spotlight.Usuario.Tests;

public sealed class TelemetriaMongoIntegrationTests : IAsyncLifetime
{
    private readonly MongoDbContainer mongo = new MongoDbBuilder("mongo:7.0")
        .Build();

    private readonly string databaseName = $"spotlight_telemetry_tests_{Guid.NewGuid():N}";
    private const string CollectionName = "interacoes";

    private WebApplicationFactory<Program>? factory;
    private IMongoCollection<BsonDocument>? collection;

    public async Task InitializeAsync()
    {
        await mongo.StartAsync();

        ConfigureEnvironment(mongo.GetConnectionString(), databaseName, CollectionName);

        factory = new WebApplicationFactory<Program>();
        var client = new MongoClient(mongo.GetConnectionString());
        collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(CollectionName);
    }

    public async Task DisposeAsync()
    {
        factory?.Dispose();
        await mongo.DisposeAsync();
        ConfigureEnvironment(string.Empty, string.Empty, string.Empty);
    }

    [MongoIntegrationFact]
    public async Task PostTelemetria_ComPayloadValido_PersisteDocumentoNoMongo()
    {
        var http = CreateClient();
        var eventId = $"evt_integracao_{Guid.NewGuid():N}";

        var response = await http.PostAsJsonAsync("/telemetria", new
        {
            eventId,
            eventType = "show_viewed",
            source = "mobile",
            sessionId = "sess_integracao",
            anonymousId = "anon_integracao",
            screen = "DetalhesView",
            entity = new { type = "espetaculo", id = "show-42" },
            properties = new { origin = "catalog" },
            device = new { platform = "ios" }
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var document = await FindByEventIdAsync(eventId);
        Assert.NotNull(document);
        Assert.Equal("show_viewed", document!["event_type"].AsString);
        Assert.Equal("mobile", document["source"].AsString);
        Assert.Equal("sess_integracao", document["session_id"].AsString);
        Assert.Equal("anon_integracao", document["anonymous_id"].AsString);
        Assert.Equal("espetaculo", document["entity"]["type"].AsString);
        Assert.Equal("show-42", document["entity"]["id"].AsString);
        Assert.True(document.Contains("occurred_at"));
        Assert.True(document.Contains("received_at"));
    }

    [MongoIntegrationFact]
    public async Task Startup_ComMongoConfigurado_CriaIndicesOperacionais()
    {
        _ = CreateClient();

        var indexNames = await GetIndexNamesAsync();

        Assert.Contains("_id_", indexNames);
        Assert.Contains("ux_event_id", indexNames);
        Assert.Contains("ix_user_time", indexNames);
        Assert.Contains("ix_anonymous_time", indexNames);
        Assert.Contains("ix_entity_time", indexNames);
        Assert.Contains("ix_event_type_time", indexNames);
        Assert.Contains("ix_session_time", indexNames);
    }

    [MongoIntegrationFact]
    public async Task PostTelemetria_ComPayloadInvalido_Retorna400ENaoGravaDocumento()
    {
        var http = CreateClient();
        var before = await CountDocumentsAsync();

        var response = await http.PostAsJsonAsync("/telemetria", new
        {
            eventId = $"evt_invalido_{Guid.NewGuid():N}",
            eventType = "search_performed",
            source = "mobile",
            sessionId = "sess_integracao",
            anonymousId = "anon_integracao",
            entity = new { type = "espetaculo", id = "show-42" },
            properties = new { }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await CountDocumentsAsync());
    }

    [MongoIntegrationFact]
    public async Task PostTelemetria_ComEventIdDuplicado_Retorna202MasNaoDuplicaDocumento()
    {
        var http = CreateClient();
        var eventId = $"evt_duplicado_{Guid.NewGuid():N}";

        var payload = new
        {
            eventId,
            eventType = "show_viewed",
            source = "mobile",
            sessionId = "sess_integracao",
            anonymousId = "anon_integracao",
            screen = "DetalhesView",
            entity = new { type = "espetaculo", id = "show-42" }
        };

        var firstResponse = await http.PostAsJsonAsync("/telemetria", payload);
        var secondResponse = await http.PostAsJsonAsync("/telemetria", payload);

        Assert.Equal(HttpStatusCode.Accepted, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, secondResponse.StatusCode);
        Assert.Equal(1, await CountByEventIdAsync(eventId));
    }

    [MongoIntegrationFact]
    public async Task PostTelemetria_ComMongoIndisponivel_Retorna202BestEffort()
    {
        ConfigureEnvironment(
            "mongodb://127.0.0.1:1/?serverSelectionTimeoutMS=200",
            $"spotlight_telemetry_unavailable_{Guid.NewGuid():N}",
            CollectionName);

        using var unavailableFactory = new WebApplicationFactory<Program>();
        var http = unavailableFactory.CreateClient();

        var response = await http.PostAsJsonAsync("/telemetria", new
        {
            eventId = $"evt_mongo_indisponivel_{Guid.NewGuid():N}",
            eventType = "show_viewed",
            source = "mobile",
            sessionId = "sess_integracao",
            anonymousId = "anon_integracao",
            screen = "DetalhesView",
            entity = new { type = "espetaculo", id = "show-42" }
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        ConfigureEnvironment(mongo.GetConnectionString(), databaseName, CollectionName);
    }

    private HttpClient CreateClient()
    {
        if (factory is null) throw new InvalidOperationException("Factory nao inicializada.");
        return factory.CreateClient();
    }

    private async Task<BsonDocument?> FindByEventIdAsync(string eventId)
    {
        var cursor = await GetCollection()
            .Find(Builders<BsonDocument>.Filter.Eq("event_id", eventId))
            .Limit(1)
            .ToListAsync();

        return cursor.SingleOrDefault();
    }

    private Task<long> CountByEventIdAsync(string eventId) =>
        GetCollection().CountDocumentsAsync(Builders<BsonDocument>.Filter.Eq("event_id", eventId));

    private Task<long> CountDocumentsAsync() =>
        GetCollection().CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty);

    private async Task<IReadOnlyCollection<string>> GetIndexNamesAsync()
    {
        using var cursor = await GetCollection().Indexes.ListAsync();
        var indexes = await cursor.ToListAsync();
        return indexes.Select(index => index["name"].AsString).ToArray();
    }

    private IMongoCollection<BsonDocument> GetCollection() =>
        collection ?? throw new InvalidOperationException("Collection nao inicializada.");

    private static void ConfigureEnvironment(
        string mongoConnectionString,
        string mongoDatabase,
        string mongoCollection)
    {
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", string.Empty);
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", string.Empty);
        Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", mongoConnectionString);
        Environment.SetEnvironmentVariable("MONGODB_DATABASE", mongoDatabase);
        Environment.SetEnvironmentVariable("MONGODB_TELEMETRY_COLLECTION", mongoCollection);
    }
}
