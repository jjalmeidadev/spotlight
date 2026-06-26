using System.Net.Http.Headers;

namespace Spotlight.Recomendacao.Infrastructure.Storage;

public sealed class SupabaseModelArtifactsSyncHostedService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<SupabaseModelArtifactsSyncHostedService> logger) : IHostedService
{
    private const string DefaultBucket = "ml-artifacts";
    private const string DefaultTargetDirectory = "model-artifacts";
    private const string DefaultModelObjectPath = "models/recomendacao/current/recomendacao_incrementada_mlnet.zip";
    private const string DefaultSimilaritiesObjectPath = "models/recomendacao/current/item_similarities.json";
    private const string DefaultCandidateCsvObjectPath = "models/recomendacao/current/recomendacao_balanceado.csv";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("SupabaseModelArtifacts:Enabled", false))
        {
            logger.LogInformation("Sincronizacao de artefatos do Supabase Storage desabilitada.");
            return;
        }

        var supabaseUrl = ReadSetting("SupabaseModelArtifacts:SupabaseUrl", "SUPABASE_URL");
        var serviceRoleKey = ReadSetting("SupabaseModelArtifacts:ServiceRoleKey", "SUPABASE_SERVICE_ROLE_KEY");

        if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(serviceRoleKey))
        {
            logger.LogWarning(
                "SupabaseModelArtifacts esta habilitado, mas SUPABASE_URL ou SUPABASE_SERVICE_ROLE_KEY nao foram configurados.");
            return;
        }

        var bucket = configuration["SupabaseModelArtifacts:Bucket"] ?? DefaultBucket;
        var targetDirectory = Path.GetFullPath(
            configuration["SupabaseModelArtifacts:TargetDirectory"] ?? DefaultTargetDirectory,
            Directory.GetCurrentDirectory());

        Directory.CreateDirectory(targetDirectory);

        var files = new[]
        {
            new ArtifactFile(
                configuration["SupabaseModelArtifacts:ModelObjectPath"] ?? DefaultModelObjectPath,
                "recomendacao_incrementada_mlnet.zip"),
            new ArtifactFile(
                configuration["SupabaseModelArtifacts:SimilaritiesObjectPath"] ?? DefaultSimilaritiesObjectPath,
                "item_similarities.json"),
            new ArtifactFile(
                configuration["SupabaseModelArtifacts:CandidateCsvObjectPath"] ?? DefaultCandidateCsvObjectPath,
                "recomendacao_balanceado.csv")
        };

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
        httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);

        foreach (var file in files)
        {
            await DownloadArtifactAsync(
                httpClient,
                supabaseUrl,
                bucket,
                file,
                targetDirectory,
                cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task DownloadArtifactAsync(
        HttpClient httpClient,
        string supabaseUrl,
        string bucket,
        ArtifactFile file,
        string targetDirectory,
        CancellationToken cancellationToken)
    {
        var objectPath = EncodeObjectPath(file.ObjectPath);
        var requestUri = $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/{bucket}/{objectPath}";
        var targetPath = Path.Combine(targetDirectory, file.FileName);
        var temporaryPath = $"{targetPath}.download";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Nao foi possivel baixar artefato {ObjectPath} do bucket {Bucket}. HTTP {StatusCode}.",
                file.ObjectPath,
                bucket,
                (int)response.StatusCode);
            return;
        }

        await using (var output = File.Create(temporaryPath))
        {
            await response.Content.CopyToAsync(output, cancellationToken);
        }

        File.Move(temporaryPath, targetPath, overwrite: true);
        logger.LogInformation(
            "Artefato {ObjectPath} sincronizado para {TargetPath}.",
            file.ObjectPath,
            targetPath);
    }

    private string? ReadSetting(string configKey, string environmentKey) =>
        configuration[configKey] ?? Environment.GetEnvironmentVariable(environmentKey);

    private static string EncodeObjectPath(string objectPath) =>
        string.Join(
            '/',
            objectPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

    private sealed record ArtifactFile(string ObjectPath, string FileName);
}
