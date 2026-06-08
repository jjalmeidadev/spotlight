using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Infrastructure.Auth;
using Spotlight.Recomendacao.Infrastructure.Geocoding;
using Spotlight.Recomendacao.Infrastructure.Persistence;
using Spotlight.Recomendacao.Presentation.Middleware;

// Carrega variáveis do arquivo .env (se existir) para o ambiente do processo.
// NoClobber: variáveis JÁ presentes no ambiente (definidas pela plataforma de
// deploy, pelo shell, ou — nos testes de integração — injetadas via
// Environment.SetEnvironmentVariable antes de o host subir) sempre vencem o
// arquivo .env. Isso é o comportamento correto tanto em produção (env vars reais
// não devem ser silenciosamente sobrescritas por um .env desatualizado) quanto
// em teste (permite ao WebApplicationFactory apontar para as dependências de
// teste — Postgres do Testcontainers, segredo JWT determinístico etc. — sem que
// o .env local "vença" e a aplicação suba apontando para o ambiente real).
Env.TraversePath().NoClobber().Load();

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
// As chaves são lidas via IConfiguration (que já agrega variáveis de ambiente),
// e não direto de Environment.GetEnvironmentVariable. Isso preserva o
// comportamento em produção (env vars continuam funcionando) e permite que os
// testes de integração injetem valores determinísticos via configuração,
// sobrescrevendo qualquer .env eventualmente carregado pelo DotNetEnv.
// ---------------------------------------------------------------------------
var connectionString =
    builder.Configuration["DATABASE_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("Default")
    ?? string.Empty;

// Base URL da API externa de geocodificação (OpenStreetMap/Nominatim).
// Default público; sobrescrevível por GEOCODING_BASE_URL (env ou config).
var geocodingBaseUrl =
    builder.Configuration["GEOCODING_BASE_URL"]
    ?? "https://nominatim.openstreetmap.org";

var geocodingUserAgent =
    builder.Configuration["GEOCODING_USER_AGENT"]
    ?? "spotlight-inteli-g04/1.0";

// ---------------------------------------------------------------------------
// Banco de dados (Npgsql / ADO.NET — sem ORM — PostgreSQL/Supabase)
// ---------------------------------------------------------------------------
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

    builder.Services.AddScoped<IRecomendacaoRepository, RecomendacaoRepository>();
    builder.Services.AddScoped<ISimilaridadeRepository, SimilaridadeRepository>();
    builder.Services.AddScoped<ILocalizacaoUsuarioRepository, LocalizacaoUsuarioRepository>();
    builder.Services.AddScoped<ListarRecomendacoesMeUseCase>();
    builder.Services.AddScoped<ListarSimilaresUseCase>();

    // Cliente HTTP tipado para o reverse geocoding (dependência consumida).
    // Timeout curto: o enriquecimento é "best effort" e não pode atrasar a
    // resposta; em falha/timeout o caso de uso degrada graciosamente (RNF03).
    builder.Services.AddHttpClient<IGeocodingClient, NominatimGeocodingClient>(client =>
    {
        client.BaseAddress = new Uri(geocodingBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(3);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(geocodingUserAgent);
    });
}

// ---------------------------------------------------------------------------
// Autenticação JWT (Supabase) + autorização por papel.
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
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Spotlight — Serviço de Recomendação",
        Version = "v1",
        Description = "Orquestra o fluxo de sugestões, consultando predições do Motor de ML."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT do Supabase Auth. Informe: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Aplica o requirement apenas em operações com [Authorize] — rotas públicas
    // (ex.: /espetaculos/{id}/similares) ficam sem cadeado na UI.
    options.OperationFilter<Spotlight.Recomendacao.Infrastructure.Auth.AuthorizeOperationFilter>();

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline de middlewares
// ---------------------------------------------------------------------------
app.UseMiddleware<ErrorHandlingMiddleware>(); // error handler global (primeiro)
app.UseSerilogRequestLogging();
app.UseCors(CorsPolicy);

// Swagger sempre habilitado: o gateway agrega as specs dos serviços em /swagger.
app.UseSwagger();
app.UseSwaggerUI();

// Auth: precisa vir antes do MapControllers para que [Authorize] funcione.
app.UseAuthentication();
app.UseAuthorization();

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
