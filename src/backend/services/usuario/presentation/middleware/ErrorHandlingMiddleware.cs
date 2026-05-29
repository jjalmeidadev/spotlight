using System.Net;
using System.Text.Json;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Presentation.Middleware;

/// <summary>
/// Traduz exceções para o envelope de erro do contrato:
/// <c>{ error, traceId, details? }</c>. Exceções de domínio viram 400/404/409;
/// qualquer outra vira 500 (sem vazar stack trace).
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await EscreverAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ValidationException ex)
        {
            await EscreverAsync(context, HttpStatusCode.BadRequest, ex.Message, ex.Details);
        }
        catch (ConflictException ex)
        {
            await EscreverAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await EscreverAsync(context, HttpStatusCode.InternalServerError, "Erro interno no servidor.");
        }
    }

    private static Task EscreverAsync(
        HttpContext context,
        HttpStatusCode status,
        string mensagem,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = details is null
            ? JsonSerializer.Serialize(new { error = mensagem, traceId = context.TraceIdentifier }, JsonOpts)
            : JsonSerializer.Serialize(new { error = mensagem, traceId = context.TraceIdentifier, details }, JsonOpts);

        return context.Response.WriteAsync(payload);
    }
}
