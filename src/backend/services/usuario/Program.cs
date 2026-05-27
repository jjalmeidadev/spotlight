using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Serilog;
using Spotlight.Usuario.Presentation.Middleware;

// Carrega variáveis do arquivo .env (se existir) para o ambiente do processo.
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
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? string.Empty;

// ---------------------------------------------------------------------------
// Banco de dados (Npgsql / ADO.NET — sem ORM — PostgreSQL/Supabase)
// ---------------------------------------------------------------------------
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
}

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
        Title = "Spotlight — Serviço de Usuário",
        Version = "v1",
        Description = "Gestão de perfis, preferências declaradas e histórico de consumo."
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

app.MapControllers();

// ---------------------------------------------------------------------------
// Rotas de health check
// ---------------------------------------------------------------------------
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

// Expõe a classe Program para o WebApplicationFactory dos testes de integração.
public partial class Program;
