using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Spotlight.Catalogo.Infrastructure.Auth;

/// <summary>
/// Adiciona o requirement de Bearer JWT apenas nas operações decoradas com
/// <see cref="AuthorizeAttribute"/> (no método ou no controller), respeitando
/// <see cref="AllowAnonymousAttribute"/>. Garante que rotas públicas no Swagger
/// fiquem sem cadeado e sem 401/403 documentados.
/// </summary>
public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpoint = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (endpoint.OfType<AllowAnonymousAttribute>().Any()) return;
        if (!endpoint.OfType<AuthorizeAttribute>().Any()) return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
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
            }
        ];
    }
}
