using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Spotlight.Recomendacao.Domain.Entities;
using Spotlight.Recomendacao.Presentation.Middleware;
using Xunit;

namespace Spotlight.Recomendacao.Tests;

/// <summary>
/// Testes unitários (caixa-branca) do <see cref="ErrorHandlingMiddleware"/>: cada
/// cenário injeta um <c>RequestDelegate</c> que lança um tipo de exceção específico
/// e verifica a tradução para o envelope <c>{ error, traceId, details? }</c> e o
/// status HTTP correspondente — incluindo o branch genérico (500), que não tem como
/// ser exercitado de ponta a ponta sem derrubar a aplicação real.
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static async Task<(int StatusCode, JsonElement Corpo)> InvocarComAsync(Exception excecaoLancada)
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw excecaoLancada,
            NullLogger<ErrorHandlingMiddleware>.Instance);

        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(contexto);

        contexto.Response.Body.Seek(0, SeekOrigin.Begin);
        using var leitor = new StreamReader(contexto.Response.Body);
        var corpo = JsonSerializer.Deserialize<JsonElement>(await leitor.ReadToEndAsync(Ct));

        return (contexto.Response.StatusCode, corpo);
    }

    [Fact]
    public async Task NotFoundException_VirA404ComEnvelopeSemDetails()
    {
        var (status, corpo) = await InvocarComAsync(new NotFoundException("Espetáculo 99 não encontrado."));

        Assert.Equal((int)HttpStatusCode.NotFound, status);
        Assert.Equal("Espetáculo 99 não encontrado.", corpo.GetProperty("error").GetString());
        Assert.True(corpo.TryGetProperty("traceId", out _));
        Assert.False(corpo.TryGetProperty("details", out _));
    }

    [Fact]
    public async Task ValidationException_VirA400ComDetailsDoCampoInvalido()
    {
        var excecao = new ValidationException(new Dictionary<string, string[]>
        {
            ["sub"] = ["Claim 'sub' ausente ou inválida no token."]
        });

        var (status, corpo) = await InvocarComAsync(excecao);

        Assert.Equal((int)HttpStatusCode.BadRequest, status);
        Assert.Equal("Dados de entrada inválidos.", corpo.GetProperty("error").GetString());
        var mensagens = corpo.GetProperty("details").GetProperty("sub").EnumerateArray()
            .Select(e => e.GetString()).ToList();
        Assert.Equal(["Claim 'sub' ausente ou inválida no token."], mensagens);
    }

    [Fact]
    public async Task ConflictException_VirA409ComEnvelopeSemDetails()
    {
        var (status, corpo) = await InvocarComAsync(new ConflictException("Recurso já existe."));

        Assert.Equal((int)HttpStatusCode.Conflict, status);
        Assert.Equal("Recurso já existe.", corpo.GetProperty("error").GetString());
        Assert.False(corpo.TryGetProperty("details", out _));
    }

    [Fact]
    public async Task ExcecaoNaoMapeada_VirA500SemVazarDetalhesInternos()
    {
        var (status, corpo) = await InvocarComAsync(new InvalidOperationException("segredo interno do banco"));

        Assert.Equal((int)HttpStatusCode.InternalServerError, status);
        Assert.Equal("Erro interno no servidor.", corpo.GetProperty("error").GetString());
        Assert.DoesNotContain("segredo", corpo.GetRawText());
    }
}
