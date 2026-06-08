using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using WireMock.Server;
using Xunit;

namespace Spotlight.Recomendacao.Tests.Integration;

/// <summary>
/// Fixture compartilhada pelos testes de integração do fluxo
/// <c>GET /recomendacao/me</c>. Sobe e mantém entre os testes:
///
/// <list type="bullet">
///   <item><description><b>Dependência possuída</b>: PostgreSQL real em container
///   (<c>Testcontainers</c>, imagem <c>postgres:15-alpine</c> — PG 15.x, compatível
///   com o Postgres do Supabase de produção e com a referência documentada em
///   <c>src/database/README.md</c> §6.1; o ambiente atual não roda um serviço de
///   banco no <c>docker-compose.yml</c> da raiz — o Postgres é o Supabase externo).
///   O schema é montado executando, em ordem, os
///   scripts oficiais <c>00_supabase_compat.sql → 01_create_database.sql</c>
///   (o <c>02_initial_data.sql</c> é deliberadamente OMITIDO — ver nota abaixo)
///   e, por fim, um seed mínimo e determinístico próprio do teste.</description></item>
///   <item><description><b>Dependência consumida</b>: <c>WireMock.Net</c> stubando a
///   API externa de geocodificação (reverse geocoding).</description></item>
/// </list>
///
/// <para><b>Por que não rodar <c>02_initial_data.sql</c></b>: esse script contém um
/// bug pré-existente (independente deste trabalho) — várias instruções
/// <c>UPDATE usuario SET localizacao = 'Sao Paulo, SP', ...</c> atribuem uma string
/// simples a uma coluna <c>JSONB</c> (ver <c>01_create_database.sql:18</c>), o que o
/// PostgreSQL rejeita com <c>invalid input syntax for type json</c>. Como o script
/// inteiro chega ao servidor como uma única mensagem multi-statement, o Postgres o
/// envolve num único bloco de transação implícito: o erro no meio derruba TODO o
/// script, inclusive os <c>INSERT</c> de espetáculos que viriam antes. Reproduzido
/// de forma isolada (fora deste fixture) rodando os três scripts do projeto, sem
/// modificação, contra um <c>postgres:15-alpine</c> limpo. Em vez de depender de um
/// script com esse problema — e de tentar "consertar" dados de seed compartilhados
/// fora do escopo aditivo deste teste — o fixture semeia diretamente, em
/// <see cref="SeedDeTeste"/>, só os registros mínimos de que os cenários precisam
/// (usuário com localização válida + espetáculos + recomendações), mantendo o setup
/// 100% determinístico e independente de mudanças futuras no seed compartilhado.</para>
///
/// A aplicação conecta-se como o role <c>postgres</c> (superuser), que <b>ignora
/// RLS</b> — fiel ao ambiente de desenvolvimento — então as queries em
/// <c>recomendacao</c> (protegida por <c>auth.uid() = id_usuario</c>) retornam linhas
/// sem precisar setar <c>request.jwt.claim.sub</c>.
/// </summary>
public sealed class IntegrationFixture : IAsyncLifetime
{
    /// <summary>Segredo HS256 de teste (≥ 32 bytes, exigência do HMAC-SHA256).</summary>
    private const string JwtSecret = "spotlight-integration-test-secret-key-0123456789-abcdef";

    /// <summary>UUID fixo do usuário semeado para os testes.</summary>
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-0000000000aa");

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("postgres")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public WireMockServer GeocodingMock { get; private set; } = null!;
    public RecomendacaoApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await MontarSchemaESeedAsync(_postgres.GetConnectionString());

        GeocodingMock = WireMockServer.Start();

        Factory = new RecomendacaoApiFactory(
            connectionString: _postgres.GetConnectionString(),
            jwtSecret: JwtSecret,
            geocodingBaseUrl: GeocodingMock.Url!);
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        GeocodingMock?.Stop();
        await _postgres.DisposeAsync();
    }

    /// <summary>Cria um HttpClient da aplicação sob teste.</summary>
    public System.Net.Http.HttpClient CreateClient() => Factory.CreateClient();

    /// <summary>
    /// Gera um JWT HS256 válido (assinado com o segredo de teste) para o usuário
    /// semeado, com o claim <c>tipo_usuario = cadastrado</c> exigido pela policy.
    /// </summary>
    public string GerarTokenUsuario()
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", TestUserId.ToString()),
                new Claim("tipo_usuario", "cadastrado")
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credenciais
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    // -----------------------------------------------------------------------
    // Bootstrap do banco
    // -----------------------------------------------------------------------
    private static async Task MontarSchemaESeedAsync(string connectionString)
    {
        var databaseDir = LocalizarDiretorioDatabase();

        await using var conexao = new NpgsqlConnection(connectionString);
        await conexao.OpenAsync();

        // Ordem obrigatória: compat (extensões + schema auth) → schema.
        // 02_initial_data.sql NÃO entra aqui de propósito — ver o porquê no
        // comentário XML da classe. O seed de que os cenários precisam é
        // montado de forma própria e determinística em SeedDeTeste().
        foreach (var arquivo in new[]
                 {
                     "00_supabase_compat.sql",
                     "01_create_database.sql"
                 })
        {
            var sql = await File.ReadAllTextAsync(Path.Combine(databaseDir, arquivo));
            await ExecutarAsync(conexao, sql);
        }

        await ExecutarAsync(conexao, SeedDeTeste());
    }

    private static async Task ExecutarAsync(NpgsqlConnection conexao, string sql)
    {
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Seed determinístico e autocontido específico do teste — não depende do
    /// <c>02_initial_data.sql</c> compartilhado (ver nota na classe). Cria, do zero:
    /// um usuário com localização (lat/lon em JSON válido), 5 espetáculos mínimos
    /// (só <c>titulo</c>, único campo <c>NOT NULL</c> sem default em <c>espetaculo</c>)
    /// e 5 recomendações (RF04 exige ≥ 5) usando apenas <c>tipo_algoritmo</c> aceito
    /// pela constraint <c>chk_tipo_algoritmo</c> (k-NN / SVD / k-means).
    /// </summary>
    private static string SeedDeTeste() => $$"""
        INSERT INTO auth.users (
            id, aud, role, email, encrypted_password,
            email_confirmed_at, raw_user_meta_data, created_at, updated_at
        ) VALUES (
            '{{TestUserId}}', 'authenticated', 'authenticated',
            'rafael.integracao@spotlight.test',
            crypt('Spotlight@2025!', gen_salt('bf', 10)),
            NOW(), '{"nome": "Rafael Integracao"}'::jsonb, NOW(), NOW()
        ) ON CONFLICT (id) DO NOTHING;

        UPDATE usuario SET
            localizacao = '{"formattedAddress":"Avenida Paulista, Sao Paulo","latitude":-23.561,"longitude":-46.656,"placeId":"101","provider":"openstreetmap_nominatim"}'::jsonb
        WHERE id_usuario = '{{TestUserId}}';

        INSERT INTO espetaculo (id_espetaculo, titulo) VALUES
            (1, 'Espetaculo de teste 1 (integracao)'),
            (2, 'Espetaculo de teste 2 (integracao)'),
            (3, 'Espetaculo de teste 3 (integracao)'),
            (4, 'Espetaculo de teste 4 (integracao)'),
            (5, 'Espetaculo de teste 5 (integracao)')
        ON CONFLICT (id_espetaculo) DO NOTHING;

        INSERT INTO recomendacao (
            id_usuario, id_espetaculo, tipo_algoritmo,
            score_recomendacao, motivo_recomendacao, prioridade_manual
        ) VALUES
            ('{{TestUserId}}', 1, 'k-NN',    0.950000, 'Afinidade com o perfil musical.', FALSE),
            ('{{TestUserId}}', 2, 'SVD',     0.900000, 'Predicao alta por fatoracao matricial.', FALSE),
            ('{{TestUserId}}', 3, 'k-NN',    0.880000, 'Vizinhos com perfil analogo.', FALSE),
            ('{{TestUserId}}', 4, 'k-means', 0.800000, 'Cluster de preferencia familiar.', FALSE),
            ('{{TestUserId}}', 5, 'SVD',     0.750000, 'Afinidade latente identificada.', FALSE)
        ON CONFLICT DO NOTHING;
        """;

    private static string LocalizarDiretorioDatabase()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidato = Path.Combine(dir.FullName, "src", "database", "01_create_database.sql");
            if (File.Exists(candidato))
            {
                return Path.Combine(dir.FullName, "src", "database");
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar 'src/database' a partir de " + AppContext.BaseDirectory);
    }
}
