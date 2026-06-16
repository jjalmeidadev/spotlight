using Xunit;

namespace Spotlight.Usuario.Tests;

public sealed class MongoIntegrationFactAttribute : FactAttribute
{
    public MongoIntegrationFactAttribute()
    {
        if (!IsEnabled())
        {
            Skip = "Defina RUN_MONGO_INTEGRATION_TESTS=true para executar os testes de integracao MongoDB.";
        }
    }

    private static bool IsEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("RUN_MONGO_INTEGRATION_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);
}
