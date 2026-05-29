using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Serilog;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Infrastructure.Auth;
using Spotlight.Catalogo.Infrastructure.Persistence;
using Spotlight.Catalogo.Presentation.Middleware;

// Carrega variáveis do arquivo .env (se existir) para o ambiente do processo.
// TraversePath() procura o .env subindo a árvore de diretórios.
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logger (Serilog)
// ---------------------------------------------------------------------------
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

// ---------------------------------------------------------------------------
// Configuração / variáveis de ambiente
// ---------------------------------------------------------------------------
// A connection string vem de DATABASE_CONNECTION_STRING (.env) e, na ausência
// dela, do appsettings.json. Mantemos string vazia como fallback para que o
// serviço suba mesmo sem banco configurado (a checagem real fica no
// endpoint /health/ready).
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? string.Empty;

// ---------------------------------------------------------------------------
// Banco de dados (Npgsql / ADO.NET — sem ORM — PostgreSQL/Supabase)
// Registramos um NpgsqlDataSource (singleton, thread-safe, com pool de conexões)
// que os repositórios de infrastructure/persistence injetam para executar SQL puro.
// Só é registrado quando há connection string, para o serviço subir mesmo sem
// banco configurado (ex.: testes) — a checagem real fica no /health/ready.
// ---------------------------------------------------------------------------
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

    // Repositórios (SQL puro Npgsql) — dependem do NpgsqlDataSource, por isso
    // só registram com banco configurado (mantém testes sem DB funcionando).
    builder.Services.AddScoped<IEspetaculoRepository, EspetaculoRepository>();
    builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
    builder.Services.AddScoped<ITeatroRepository, TeatroRepository>();
    builder.Services.AddScoped<ISessaoRepository, SessaoRepository>();
    builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();

    // Casos de uso das features do catálogo.
    builder.Services.AddScoped<ListarEspetaculosUseCase>();
    builder.Services.AddScoped<ObterEspetaculoUseCase>();
    builder.Services.AddScoped<ListarCategoriasUseCase>();
    builder.Services.AddScoped<ListarTeatrosUseCase>();
    builder.Services.AddScoped<ObterTeatroUseCase>();
    builder.Services.AddScoped<ListarSessoesUseCase>();
    builder.Services.AddScoped<ListarAvaliacoesUseCase>();
    builder.Services.AddScoped<CriarAvaliacaoUseCase>();
    builder.Services.AddScoped<CriarEspetaculoUseCase>();
    builder.Services.AddScoped<AtualizarEspetaculoUseCase>();
    builder.Services.AddScoped<ExcluirEspetaculoUseCase>();
}

// ---------------------------------------------------------------------------
// Autenticação JWT (Supabase) + autorização por papel
// Só registra se SUPABASE_JWT_SECRET estiver definido (ver AuthSetup).
// ---------------------------------------------------------------------------
builder.AdicionarAutenticacaoESupabase();

// ---------------------------------------------------------------------------
// Health checks: liveness (sempre 200) + readiness (checa o Postgres)
// ---------------------------------------------------------------------------
var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);
}
else
{
    // Sem connection string o serviço NÃO está pronto para tráfego —
    // readiness deve falhar (evita falso positivo no /health/ready).
    healthChecks.AddCheck("postgres", () =>
        HealthCheckResult.Unhealthy("Connection string do banco não configurada."),
        tags: ["ready"]);
}

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
const string CorsPolicy = "SpotlightCors";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ---------------------------------------------------------------------------
// Controllers + Swagger / OpenAPI
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new()
    {
        Title = "Spotlight — Serviço de Catálogo",
        Version = "v1",
        Description = "Gerenciamento de espetáculos, teatros, gêneros e precificação."
    }));

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline de middlewares
// ---------------------------------------------------------------------------
app.UseMiddleware<ErrorHandlingMiddleware>(); // error handler global (primeiro)
app.UseSerilogRequestLogging();
app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auth: precisa vir antes do MapControllers para que [Authorize] funcione.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------------------------------------------------------------------------
// Rotas de health check
// ---------------------------------------------------------------------------
// Liveness: confirma que o processo está de pé (não toca no banco).
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: 200 somente quando o banco responde.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

// Expõe a classe Program para o WebApplicationFactory dos testes de integração.
public partial class Program;
