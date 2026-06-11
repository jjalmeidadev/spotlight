using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Spotlight.Recomendacao.Application.UseCases;

namespace Spotlight.Recomendacao.Infrastructure.Auth;

/// <summary>
/// Configuracao de autenticacao/autorizacao para o servico de Recomendacao.
/// Supabase Auth pode validar tokens pelo segredo JWT local ou pelas chaves JWKS
/// publicas do projeto Supabase.
/// </summary>
public static class AuthSetup
{
    public const string PolicyCadastrado = "Cadastrado";
    public const string PolicyAdministrador = "Administrador";

    private const string ClaimTipoUsuario = "tipo_usuario";

    public static void AdicionarAutenticacaoESupabase(this WebApplicationBuilder builder)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");
        var jwksUrl =
            Environment.GetEnvironmentVariable("SUPABASE_AUTH_JWKS_URL")
            ?? BuildJwksUrl(Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_URL"));

        void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(PolicyCadastrado, p => p.RequireAssertion(ctx =>
                ctx.User.HasClaim(ClaimTipoUsuario, "cadastrado") ||
                ctx.User.HasClaim(ClaimTipoUsuario, "administrador")));

            options.AddPolicy(PolicyAdministrador, p => p.RequireClaim(ClaimTipoUsuario, "administrador"));
        }

        if (string.IsNullOrWhiteSpace(jwtSecret) && string.IsNullOrWhiteSpace(jwksUrl))
        {
            builder.Services.AddAuthorization(AddPolicies);
            return;
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUsuarioContexto, UsuarioContextoHttp>();

        var chaves = GetSigningKeys(jwtSecret, jwksUrl);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = chaves,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        ctx.HttpContext.Items["auth_error"] = ctx.Exception.Message;
                        return Task.CompletedTask;
                    },
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        var details = builder.Environment.IsDevelopment()
                            ? ctx.HttpContext.Items["auth_error"] as string
                            : null;

                        await EscreverErroAsync(ctx.Response, StatusCodes.Status401Unauthorized,
                            "Token ausente ou invalido.", ctx.HttpContext.TraceIdentifier, details);
                    },
                    OnForbidden = async ctx =>
                    {
                        await EscreverErroAsync(ctx.Response, StatusCodes.Status403Forbidden,
                            "Usuario sem permissao para esta operacao.", ctx.HttpContext.TraceIdentifier);
                    }
                };
            });

        builder.Services.AddAuthorization(AddPolicies);
    }

    private static Task EscreverErroAsync(
        HttpResponse response,
        int status,
        string mensagem,
        string traceId,
        string? details = null)
    {
        response.Clear();
        response.StatusCode = status;
        response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(
            new { error = mensagem, traceId, details },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return response.WriteAsync(payload);
    }

    private static IReadOnlyList<SecurityKey> GetSigningKeys(string? jwtSecret, string? jwksUrl)
    {
        var keys = new List<SecurityKey>();

        if (!string.IsNullOrWhiteSpace(jwtSecret))
        {
            keys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)));

            try
            {
                var decodedSecret = Convert.FromBase64String(jwtSecret);
                keys.Add(new SymmetricSecurityKey(decodedSecret));
            }
            catch (FormatException)
            {
                // Supabase projects can expose the JWT secret as plain text or base64.
            }
        }

        if (!string.IsNullOrWhiteSpace(jwksUrl))
        {
            try
            {
                var jwks = new HttpClient().GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                keys.AddRange(new JsonWebKeySet(jwks).Keys);
            }
            catch (HttpRequestException)
            {
                // Local environments can block external calls during startup.
            }
            catch (SocketException)
            {
                // Same reason above, but some network blocks surface as socket errors.
            }
        }

        return keys;
    }

    private static string? BuildJwksUrl(string? supabaseUrl)
    {
        if (string.IsNullOrWhiteSpace(supabaseUrl))
        {
            return null;
        }

        return $"{supabaseUrl.TrimEnd('/')}/auth/v1/.well-known/jwks.json";
    }
}
