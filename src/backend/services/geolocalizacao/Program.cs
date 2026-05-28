using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Spotlight.Geolocalizacao.Application.UseCases;
using Spotlight.Geolocalizacao.Presentation.Middleware;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

var nominatimBaseUrl =
    Environment.GetEnvironmentVariable("NOMINATIM_BASE_URL")
    ?? builder.Configuration["Nominatim:BaseUrl"]
    ?? "https://nominatim.openstreetmap.org";

var nominatimUserAgent =
    Environment.GetEnvironmentVariable("NOMINATIM_USER_AGENT")
    ?? builder.Configuration["Nominatim:UserAgent"]
    ?? "spotlight-inteli-g04/1.0";

builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
{
    client.BaseAddress = new Uri(nominatimBaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(nominatimUserAgent);
});

builder.Services.AddHealthChecks();

const string CorsPolicy = "SpotlightCors";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new()
    {
        Title = "Spotlight — Servico de Geolocalizacao",
        Version = "v1",
        Description = "Normalizacao de enderecos e coordenadas via OpenStreetMap/Nominatim."
    }));

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "geolocalizacao",
    status = "running",
    endpoints = new
    {
        health = "/health",
        readiness = "/health/ready",
        ping = "/api/ping",
        geocoding = "/api/geocoding?address=Sao Paulo, SP",
        reverse = "/api/geocoding/reverse?latitude=-23.55052&longitude=-46.633308",
        swagger = "/swagger"
    }
}));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();

public partial class Program;
