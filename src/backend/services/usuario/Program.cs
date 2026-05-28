using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Infrastructure.Auth;
using Spotlight.Usuario.Presentation.Middleware;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? string.Empty;

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
}

builder.Services.AddScoped<IUsuarioLocalizacaoService, UsuarioLocalizacaoService>();
builder.AdicionarAutenticacaoESupabase();

var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);
}
else
{
    healthChecks.AddCheck("postgres", () =>
        HealthCheckResult.Unhealthy("Connection string do banco nao configurada."),
        tags: ["ready"]);
}

const string CorsPolicy = "SpotlightCors";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Spotlight - Servico de Usuario",
        Version = "v1",
        Description = "Gestao de perfis, preferencias declaradas e historico de consumo."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT do Supabase Auth. Informe: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "usuario",
    status = "running",
    endpoints = new
    {
        health = "/health",
        readiness = "/health/ready",
        ping = "/api/ping",
        localizacao = "/me/localizacao",
        swagger = "/swagger"
    }
}));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public partial class Program;
