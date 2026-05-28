using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Spotlight.Usuario.Application.UseCases;

namespace Spotlight.Usuario.Infrastructure.Auth;

/// <summary>
/// Configuração de autenticação/autorização para o serviço de Usuário.
/// Supabase Auth emite JWTs HS256 assinados com o segredo do projeto
/// (env <c>SUPABASE_JWT_SECRET</c>). As policies <c>Cadastrado</c> e
/// <c>Administrador</c> validam o claim custom <c>tipo_usuario</c> — esse claim
/// precisa ser emitido pelo Supabase (via Auth Hook / Database Trigger).
///
/// Sem segredo configurado, o serviço sobe sem auth (rotas com
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
        if (string.IsNullOrWhiteSpace(jwtSecret)) return;

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUsuarioContexto, UsuarioContextoHttp>();

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = chave,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        await EscreverErroAsync(ctx.Response, StatusCodes.Status401Unauthorized,
                            "Token ausente ou inválido.", ctx.HttpContext.TraceIdentifier);
                    },
                    OnForbidden = async ctx =>
                    {
                        await EscreverErroAsync(ctx.Response, StatusCodes.Status403Forbidden,
                            "Usuário sem permissão para esta operação.", ctx.HttpContext.TraceIdentifier);
                    }
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyCadastrado, p => p.RequireAssertion(ctx =>
                ctx.User.HasClaim(ClaimTipoUsuario, "cadastrado") ||
                ctx.User.HasClaim(ClaimTipoUsuario, "administrador")));

            options.AddPolicy(PolicyAdministrador, p => p.RequireClaim(ClaimTipoUsuario, "administrador"));
        });
    }

    private static Task EscreverErroAsync(HttpResponse response, int status, string mensagem, string traceId)
    {
        response.Clear();
        response.StatusCode = status;
        response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(
            new { error = mensagem, traceId },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return response.WriteAsync(payload);
    }
}
