using DotNetEnv;
using Serilog;
using Spotlight.Gateway.Middleware;

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
// Health check (apenas liveness — o gateway não acessa banco diretamente)
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks();

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
const string CorsPolicy = "SpotlightCors";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ---------------------------------------------------------------------------
// Reverse proxy (YARP) — roteamento para os serviços de negócio.
// As rotas/clusters são lidos da seção "ReverseProxy" do appsettings; a
// configuração real (destinos, validação de JWT, rate limiting) entra posteriormente.
// ---------------------------------------------------------------------------
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new()
    {
        Title = "Spotlight — API Gateway",
        Version = "v1",
        Description = "Ponto único de entrada: roteamento para os serviços e validação de JWT."
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

// Liveness do gateway (não toca em serviços downstream).
app.MapHealthChecks("/health");

// Encaminha as demais rotas para os serviços conforme a configuração do YARP.
app.MapReverseProxy();

app.Run();

// Expõe a classe Program para o WebApplicationFactory dos testes de integração.
public partial class Program;
