using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spotlight.Catalogo.Tests.Integration;

public sealed class CatalogoIntegrationFixture : IAsyncLifetime
{
    public const string JwtSecret = "spotlight-integration-test-jwt-secret-key!!";

    private PostgreSqlContainer? _postgres;

    public CatalogoIntegrationWebApplicationFactory Factory { get; private set; } = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("spotlight_catalogo_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgres.StartAsync();
        ConnectionString = _postgres.GetConnectionString();

        await InicializarBancoAsync();

        Factory = new CatalogoIntegrationWebApplicationFactory(ConnectionString, JwtSecret);
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    private async Task InicializarBancoAsync()
    {
        var databaseDir = LocalizarDiretorioDatabase();

        await ExecutarScriptAsync(Path.Combine(databaseDir, "00_supabase_compat.sql"));
        await ExecutarScriptAsync(Path.Combine(databaseDir, "01_create_database.sql"));

        await ExecutarScriptAsync("""
            INSERT INTO categoria (nome, descricao)
            VALUES ('Categoria Integracao', 'Categoria para testes de integracao');

            INSERT INTO ator (nome, biografia)
            VALUES ('Ator Integracao', 'Ator para testes de integracao');
            """);
    }

    private async Task ExecutarScriptAsync(string caminhoOuSql)
    {
        var sql = File.Exists(caminhoOuSql)
            ? await File.ReadAllTextAsync(caminhoOuSql)
            : caminhoOuSql;

        await using var conexao = new NpgsqlConnection(ConnectionString);
        await conexao.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conexao);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string LocalizarDiretorioDatabase()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null)
        {
            var candidato = Path.Combine(diretorio.FullName, "src", "database");

            if (Directory.Exists(candidato))
            {
                return candidato;
            }

            diretorio = diretorio.Parent;
        }

        throw new InvalidOperationException(
            "Não foi possível localizar o diretório src/database a partir do diretório de testes.");
    }
}