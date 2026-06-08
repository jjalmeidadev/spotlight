using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Spotlight.Recomendacao.Tests.Integration;

/// <summary>
/// <c>WebApplicationFactory</c> que sobe o serviço <c>Spotlight.Recomendacao</c>
/// inteiro (pipeline real, DI real, controllers reais) apontando para as
/// dependências de teste:
///
/// <list type="bullet">
///   <item><description><b>DATABASE_CONNECTION_STRING</b> → o Postgres do
///   Testcontainers (dependência possuída).</description></item>
///   <item><description><b>SUPABASE_JWT_SECRET</b> → segredo de teste, para que
///   os JWTs gerados por <see cref="IntegrationFixture.GerarTokenUsuario"/>
///   sejam aceitos.</description></item>
///   <item><description><b>GEOCODING_BASE_URL</b> → a URL do
///   <c>WireMockServer</c> (dependência consumida), no lugar do Nominatim
///   real.</description></item>
/// </list>
///
/// <para><b>Por que via variável de ambiente, e não <c>ConfigureAppConfiguration</c></b>:
/// <c>Program.cs</c> lê <c>builder.Configuration[...]</c> de forma síncrona, logo no
/// início do arquivo (antes de <c>builder.Build()</c>), para montar a connection
/// string, o segredo JWT e o cliente HTTP de geocodificação. Os hooks de
/// <c>WebApplicationFactory.ConfigureWebHost</c> (incluindo
/// <c>ConfigureAppConfiguration</c>) só são aplicados durante <c>Build()</c> — tarde
/// demais para influenciar essas leituras, que já capturaram os valores reais do
/// <c>.env</c>. Por isso a injeção é feita via <c>Environment.SetEnvironmentVariable</c>
/// no construtor, ANTES de o host (e, com ele, <c>Program.cs</c>) ser criado —
/// somado ao ajuste em <c>Program.cs</c> que troca <c>Env.Load()</c> por
/// <c>Env.NoClobber().Load()</c>, para que essas variáveis sobrevivam ao
/// carregamento do <c>.env</c> local.</para>
/// </summary>
public sealed class RecomendacaoApiFactory : WebApplicationFactory<Program>
{
    public RecomendacaoApiFactory(string connectionString, string jwtSecret, string geocodingBaseUrl)
    {
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", connectionString);
        Environment.SetEnvironmentVariable("SUPABASE_JWT_SECRET", jwtSecret);
        Environment.SetEnvironmentVariable("GEOCODING_BASE_URL", geocodingBaseUrl);
        Environment.SetEnvironmentVariable("GEOCODING_USER_AGENT", "spotlight-integration-tests/1.0");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development");
}
