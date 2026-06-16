using MongoDB.Bson;
using Spotlight.Usuario.Infrastructure.Persistence;
using Xunit;

namespace Spotlight.Usuario.Tests;

public class MongoTelemetryIndexesTests
{
    [Fact]
    public void Definitions_IncluemIndicesOperacionaisDaTelemetria()
    {
        var definitions = MongoTelemetryIndexes.Definitions;

        Assert.Collection(
            definitions,
            index =>
            {
                Assert.Equal("ux_event_id", index.Name);
                Assert.Equal(new BsonDocument("event_id", 1), index.Keys);
                Assert.True(index.Unique);
                Assert.True(index.Sparse);
            },
            index =>
            {
                Assert.Equal("ix_user_time", index.Name);
                Assert.Equal(new BsonDocument
                {
                    ["user_id"] = 1,
                    ["occurred_at"] = -1
                }, index.Keys);
            },
            index =>
            {
                Assert.Equal("ix_anonymous_time", index.Name);
                Assert.Equal(new BsonDocument
                {
                    ["anonymous_id"] = 1,
                    ["occurred_at"] = -1
                }, index.Keys);
            },
            index =>
            {
                Assert.Equal("ix_entity_time", index.Name);
                Assert.Equal(new BsonDocument
                {
                    ["entity.type"] = 1,
                    ["entity.id"] = 1,
                    ["occurred_at"] = -1
                }, index.Keys);
            },
            index =>
            {
                Assert.Equal("ix_event_type_time", index.Name);
                Assert.Equal(new BsonDocument
                {
                    ["event_type"] = 1,
                    ["occurred_at"] = -1
                }, index.Keys);
            },
            index =>
            {
                Assert.Equal("ix_session_time", index.Name);
                Assert.Equal(new BsonDocument
                {
                    ["session_id"] = 1,
                    ["occurred_at"] = 1
                }, index.Keys);
            });
    }

    [Fact]
    public void CreateModels_PreservaNomeEOpcoesDosIndices()
    {
        var models = MongoTelemetryIndexes.CreateModels();

        Assert.Equal(MongoTelemetryIndexes.Definitions.Count, models.Count);
        Assert.Equal("ux_event_id", models[0].Options?.Name);
        Assert.True(models[0].Options?.Unique);
        Assert.True(models[0].Options?.Sparse);

        foreach (var model in models.Skip(1))
        {
            Assert.False(model.Options?.Unique);
            Assert.False(model.Options?.Sparse);
        }
    }
}
