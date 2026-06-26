using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Spotlight.Usuario.Application.UseCases;

namespace Spotlight.Usuario.Infrastructure.Auth;

/// <summary>
/// Configuração de autenticação/autorização para o serviço de Usuário.
/// Supabase Auth emite JWTs HS256 assinados com o segredo do projeto
/// (env <c>SUPABASE_JWT_SECRET</c>) ou via JWKS público (env
/// <c>SUPABASE_AUTH_JWKS_URL</c> / derivado de <c>SUPABASE_URL</c>).
/// As policies <c>Cadastrado</c> e <c>Administrador</c> validam o claim custom
/// <c>tipo_usuario</c> — esse claim precisa ser emitido pelo Supabase
/// (via Auth Hook / Database Trigger).
///
/// Sem segredo nem JWKS configurados, o serviço sobe sem auth (rotas com
/// <c>[Authorize]</c> devolvem 401). Esse modo é útil para testes locais sem Supabase.
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
            ?? BuildJwksUrl(FirstNotBlank(
                Environment.GetEnvironmentVariable("SUPABASE_URL"),
                Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_URL")));

        void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(PolicyCadastrado, p => p.RequireAssertion(ctx =>
                TemTipoUsuario(ctx, "cadastrado") ||
                TemTipoUsuario(ctx, "administrador")));

            options.AddPolicy(PolicyAdministrador, p => p.RequireAssertion(ctx =>
                TemTipoUsuario(ctx, "administrador")));
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
                    IssuerSigningKeyResolver = (_, _, _, _) => chaves,
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
                            "Token ausente ou inválido.", ctx.HttpContext.TraceIdentifier, details);
                    },
                    OnForbidden = async ctx =>
                    {
                        await EscreverErroAsync(ctx.Response, StatusCodes.Status403Forbidden,
                            "Usuário sem permissão para esta operação.", ctx.HttpContext.TraceIdentifier);
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
                // O ambiente local pode bloquear chamadas externas na inicializacao.
                // Mantem o servico de pe usando as chaves locais ja configuradas.
            }
            catch (SocketException)
            {
                // Mesmo motivo acima, mas alguns bloqueios chegam como erro de socket.
            }
            catch (UriFormatException)
            {
                // jwksUrl mal formada (ex.: env sem schema) — segue sem chaves JWKS.
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

    private static string? FirstNotBlank(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool TemTipoUsuario(AuthorizationHandlerContext ctx, string tipo)
    {
        if (ctx.User.HasClaim(ClaimTipoUsuario, tipo))
        {
            return true;
        }

        var raw = ctx.User.FindFirst("app_metadata")?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            return TryGetString(document.RootElement, "tipoUsuario", out var tipoUsuario) && tipoUsuario == tipo
                || TryGetString(document.RootElement, "tipo_usuario", out var tipoUsuarioSnake) && tipoUsuarioSnake == tipo;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string? value)
    {
        value = element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
        return !string.IsNullOrWhiteSpace(value);
    }
}
