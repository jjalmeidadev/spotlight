using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Spotlight.Catalogo.Tests.Integration;

public sealed class CatalogoIntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _jwtSecret;

    public CatalogoIntegrationWebApplicationFactory(string connectionString, string jwtSecret)
    {
        _connectionString = connectionString;
        _jwtSecret = jwtSecret;

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _connectionString);
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", _jwtSecret);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _connectionString
            });
        });
    }
}