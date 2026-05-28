using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Spotlight.Catalogo.Application.UseCases;

namespace Spotlight.Catalogo.Infrastructure.Auth;

/// <summary>
/// Configuração de autenticação/autorização para o serviço de Catálogo.
///
/// <para>
/// <b>Modelo:</b> Supabase Auth emite JWTs HS256 assinados com o segredo do projeto
/// (env <c>SUPABASE_JWT_SECRET</c>). As políticas <c>Cadastrado</c> e
/// <c>Administrador</c> validam o claim custom <c>tipo_usuario</c> — esse claim
/// precisa ser emitido pelo Supabase (via Auth Hook / Database Trigger que copie
/// <c>usuario.tipo_usuario</c> para o JWT). Se o claim não estiver presente, a
/// política falha e devolve <b>403</b>.
/// </para>
///
/// <para>
/// Sem token / token inválido → <b>401</b>. Ambos os casos retornam o envelope
/// padrão do contrato via os eventos <c>OnChallenge</c> e <c>OnForbidden</c>.
/// </para>
/// </summary>
public static class AuthSetup
{
    public const string PolicyCadastrado = "Cadastrado";
    public const string PolicyAdministrador = "Administrador";

    private const string ClaimTipoUsuario = "tipo_usuario";

    public static void AdicionarAutenticacaoESupabase(this WebApplicationBuilder builder)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");

        // Sem segredo configurado o serviço sobe sem auth (útil para testes/dev sem Supabase).
        // Nesse modo, rotas com [Authorize] devolvem 401 (não há ticket emitido).
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
                    ValidateIssuer = false, // Supabase varia por projeto — o segredo já garante origem.
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                // Customiza 401/403 para usar o envelope padrão do contrato.
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
            // "Cadastrado" inclui também administrador (admin pode tudo que cadastrado pode).
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
