using System.Net;
using System.Text.Json;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Presentation.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = GetStatusCode(ex);

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(ex, "Erro nao tratado ao processar {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning(ex, "Falha de negocio ao processar {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = JsonSerializer.Serialize(new
            {
                error = GetMessage(ex, statusCode),
                traceId = context.TraceIdentifier,
                details = (ex as ValidationException)?.Details
            }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            await context.Response.WriteAsync(payload);
        }
    }

    private static HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ValidationException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        NotFoundException => HttpStatusCode.NotFound,
        ConflictException => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };

    private static string GetMessage(Exception ex, HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.InternalServerError
            ? "Erro interno no servidor."
            : ex.Message;
}
