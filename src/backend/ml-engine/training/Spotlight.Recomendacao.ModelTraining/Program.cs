using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ML;
using Microsoft.ML.Data;

var options = TrainingOptions.Parse(args);
Directory.CreateDirectory(options.ArtifactsDir);
Directory.CreateDirectory(options.ReportsDir);

var relationalRows = LoadRelationalRows(options.PostgresCsv);
var mongoInteractions = LoadMongoInteractions(options.MongoJson);
var dataset = BuildTrainingRows(relationalRows, mongoInteractions);

var ml = new MLContext(seed: 42);
var data = ml.Data.LoadFromEnumerable(dataset);
var split = ml.Data.TrainTestSplit(data, testFraction: 0.2, seed: 42);

var pipeline = ml.Transforms.Concatenate(
        "Features",
        nameof(ModelInput.ScoreImplicito),
        nameof(ModelInput.MongoInteractionScore),
        nameof(ModelInput.MongoInteractionCount),
        nameof(ModelInput.UserRatingMean),
        nameof(ModelInput.UserInteractions),
        nameof(ModelInput.ItemRatingMean),
        nameof(ModelInput.ItemInteractions),
        nameof(ModelInput.ItemSimilarityScore))
    .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: nameof(ModelInput.Label),
        featureColumnName: "Features"));

var model = pipeline.Fit(split.TrainSet);
var predictions = model.Transform(split.TestSet);
var metrics = ml.BinaryClassification.Evaluate(predictions, labelColumnName: nameof(ModelInput.Label));
var scoredRows = ml.Data
    .CreateEnumerable<ScoredRecommendation>(predictions, reuseRowObject: false)
    .ToList();

var topK = CalculateTopK(scoredRows, k: 10);
var modelPath = Path.Combine(options.ArtifactsDir, "recomendacao_incrementada_mlnet.zip");
var similaritiesPath = Path.Combine(options.ArtifactsDir, "item_similarities.json");
var metricsPath = Path.Combine(options.ReportsDir, "recomendacao_mlnet_metrics.json");
var reportPath = Path.Combine(options.ReportsDir, "README_modelo_classificacao_recomendacao.md");
var examplesPath = Path.Combine(options.ReportsDir, "recomendacao_mlnet_exemplos.csv");

ml.Model.Save(model, split.TrainSet.Schema, modelPath);

var similarities = CalculateItemSimilarities(relationalRows, neighborsPerItem: 5);
WriteJson(similaritiesPath, similarities);

var metricsReport = new MetricsReport
{
    Dataset = new DatasetReport
    {
        PostgresCsv = RelativePath(options.PostgresCsv),
        MongoJson = RelativePath(options.MongoJson),
        Registros = dataset.Count,
        EventosMongoAproveitados = mongoInteractions.Count,
        TaxaPositiva = dataset.Count == 0 ? 0 : dataset.Count(row => row.Label) / (double)dataset.Count,
        Train = (int)(dataset.Count * 0.8),
        Test = dataset.Count - (int)(dataset.Count * 0.8)
    },
    ReferenciaFunkSvdNotebook = new FunkSvdReference
    {
        RmseAproximado = 1.18,
        MaeAproximado = 0.97,
        Fonte = "src/models/sistema_recomendacao_svd.ipynb"
    },
    ModeloIncrementadoMlNet = new ModelMetrics
    {
        Accuracy = metrics.Accuracy,
        Precision = metrics.PositivePrecision,
        Recall = metrics.PositiveRecall,
        F1 = metrics.F1Score,
        RocAuc = metrics.AreaUnderRocCurve
    },
    TopK = topK,
    Features =
    [
        "score_implicito",
        "mongo_interaction_score",
        "mongo_interaction_count",
        "user_rating_mean",
        "user_interactions",
        "item_rating_mean",
        "item_interactions",
        "item_similarity_score"
    ],
    Artefatos = new ArtifactsReport
    {
        Modelo = RelativePath(modelPath),
        Similaridades = RelativePath(similaritiesPath),
        Metricas = RelativePath(metricsPath),
        Relatorio = RelativePath(reportPath),
        Exemplos = RelativePath(examplesPath),
        Treinamento = "src/backend/ml-engine/training/Spotlight.Recomendacao.ModelTraining"
    }
};

WriteJson(metricsPath, metricsReport);
WriteExamples(examplesPath, scoredRows);
WriteMarkdownReport(reportPath, metricsReport);

Console.WriteLine($"Modelo ML.NET salvo em {modelPath}");
Console.WriteLine($"Metricas salvas em {metricsPath}");
Console.WriteLine($"Similaridades kNN salvas em {similaritiesPath}");

static List<RelationalRow> LoadRelationalRows(string path)
{
    var lines = File.ReadAllLines(path);
    return lines
        .Skip(1)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line =>
        {
            var parts = line.Split(',');
            return new RelationalRow(
                int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture));
        })
        .ToList();
}

static List<MongoInteraction> LoadMongoInteractions(string path)
{
    if (!File.Exists(path))
    {
        return [];
    }

    var interactions = new List<MongoInteraction>();
    foreach (var line in File.ReadLines(path))
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        if (!root.TryGetProperty("entity", out var entity))
        {
            continue;
        }

        var entityType = entity.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
        if (entityType != "espetaculo")
        {
            continue;
        }

        var userId = ParseInt(root.GetProperty("userId").GetString());
        var itemId = ParseInt(entity.GetProperty("id").GetString());
        var eventType = root.GetProperty("eventType").GetString() ?? string.Empty;
        var weight = EventWeight(eventType);

        if (userId is null || itemId is null || weight <= 0)
        {
            continue;
        }

        interactions.Add(new MongoInteraction(userId.Value, itemId.Value, weight));
    }

    return interactions;
}

static int? ParseInt(string? value) =>
    int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

static float EventWeight(string eventType) => eventType switch
{
    "screen_viewed" => 0.05f,
    "show_card_clicked" => 0.35f,
    "show_viewed" => 0.45f,
    "favorite_toggled" => 0.75f,
    "purchase_clicked" => 1.0f,
    _ => 0.0f
};

static List<ModelInput> BuildTrainingRows(
    IReadOnlyList<RelationalRow> relationalRows,
    IReadOnlyList<MongoInteraction> mongoInteractions)
{
    var userStats = relationalRows
        .GroupBy(row => row.UserId)
        .ToDictionary(
            group => group.Key,
            group => new AggregateStats(group.Average(row => row.Rating), group.Count()));

    var itemStats = relationalRows
        .GroupBy(row => row.ItemId)
        .ToDictionary(
            group => group.Key,
            group => new AggregateStats(group.Average(row => row.Rating), group.Count()));

    var mongoStats = mongoInteractions
        .GroupBy(row => (row.UserId, row.ItemId))
        .ToDictionary(
            group => group.Key,
            group => new AggregateStats(group.Sum(row => row.Score), group.Count()));

    var itemSimilarityScores = relationalRows
        .GroupBy(row => row.ItemId)
        .ToDictionary(
            group => group.Key,
            group => group.Average(row => row.ScoreImplicito));

    return relationalRows.Select(row =>
    {
        mongoStats.TryGetValue((row.UserId, row.ItemId), out var mongo);
        return new ModelInput
        {
            UserId = row.UserId,
            ItemId = row.ItemId,
            Label = row.Rating >= 4.0f,
            ScoreImplicito = row.ScoreImplicito,
            MongoInteractionScore = mongo?.AverageOrSum ?? 0.0f,
            MongoInteractionCount = mongo?.Count ?? 0,
            UserRatingMean = userStats[row.UserId].AverageOrSum,
            UserInteractions = userStats[row.UserId].Count,
            ItemRatingMean = itemStats[row.ItemId].AverageOrSum,
            ItemInteractions = itemStats[row.ItemId].Count,
            ItemSimilarityScore = itemSimilarityScores[row.ItemId]
        };
    }).ToList();
}

static TopKMetrics CalculateTopK(IReadOnlyList<ScoredRecommendation> rows, int k)
{
    var groups = rows.GroupBy(row => row.UserId).ToList();
    if (groups.Count == 0)
    {
        return new TopKMetrics();
    }

    var precisions = new List<double>();
    var recalls = new List<double>();
    var hits = new List<double>();

    foreach (var group in groups)
    {
        var relevantTotal = group.Count(row => row.Label);
        if (relevantTotal == 0)
        {
            continue;
        }

        var top = group.OrderByDescending(row => row.Probability).Take(k).ToList();
        var relevantTop = top.Count(row => row.Label);
        precisions.Add(relevantTop / (double)Math.Max(1, top.Count));
        recalls.Add(relevantTop / (double)relevantTotal);
        hits.Add(relevantTop > 0 ? 1.0 : 0.0);
    }

    return new TopKMetrics
    {
        PrecisionAt10 = precisions.Count == 0 ? 0 : precisions.Average(),
        RecallAt10 = recalls.Count == 0 ? 0 : recalls.Average(),
        HitRateAt10 = hits.Count == 0 ? 0 : hits.Average()
    };
}

static Dictionary<int, List<ItemNeighbor>> CalculateItemSimilarities(
    IReadOnlyList<RelationalRow> rows,
    int neighborsPerItem)
{
    var itemStats = rows
        .GroupBy(row => row.ItemId)
        .Select(group => new
        {
            ItemId = group.Key,
            RatingMean = group.Average(row => row.Rating),
            ScoreMean = group.Average(row => row.ScoreImplicito)
        })
        .ToList();

    return itemStats.ToDictionary(
        item => item.ItemId,
        item => itemStats
            .Where(other => other.ItemId != item.ItemId)
            .Select(other =>
            {
                var distance = Math.Abs(item.RatingMean - other.RatingMean)
                    + Math.Abs(item.ScoreMean - other.ScoreMean);
                return new ItemNeighbor(other.ItemId, 1.0 / (1.0 + distance));
            })
            .OrderByDescending(neighbor => neighbor.Similarity)
            .Take(neighborsPerItem)
            .ToList());
}

static void WriteExamples(string path, IReadOnlyList<ScoredRecommendation> rows)
{
    var builder = new StringBuilder();
    builder.AppendLine("user_id,item_id,label,probability,score_implicito");
    foreach (var row in rows.OrderByDescending(row => row.Probability).Take(20))
    {
        builder.AppendLine(string.Join(
            ',',
            row.UserId,
            row.ItemId,
            row.Label ? 1 : 0,
            row.Probability.ToString("0.####", CultureInfo.InvariantCulture),
            row.ScoreImplicito.ToString("0.####", CultureInfo.InvariantCulture)));
    }

    File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
}

static void WriteMarkdownReport(string path, MetricsReport report)
{
    var markdown = $$"""
    # Modelo de Classificacao de Recomendacao

    ## Objetivo

    Evoluir o modelo de recomendacao da Sprint 3 para um artefato consumivel diretamente pela API .NET. O incremento usa ML.NET para classificar pares usuario-espetaculo como recomendaveis e preserva um artefato auxiliar de similaridade item-item.

    ## Fontes de dados

    - PostgreSQL exportado em CSV: `{{report.Dataset.PostgresCsv}}`
    - MongoDB exportado em JSONL: `{{report.Dataset.MongoJson}}`
    - Registros usados: `{{report.Dataset.Registros}}`
    - Eventos MongoDB aproveitados: `{{report.Dataset.EventosMongoAproveitados}}`
    - Taxa positiva: `{{Percent(report.Dataset.TaxaPositiva)}}`

    ## Incremento aplicado

    O Funk SVD original continua sendo a referencia historica para predicao de rating em matriz esparsa, com RMSE aproximado de `1,18` e MAE aproximado de `0,97`, conforme documentado no notebook `src/models/sistema_recomendacao_svd.ipynb`.

    A versao incrementada passa a gerar um modelo ML.NET em `.zip`, que pode ser carregado diretamente pelo backend C# na T26. O pipeline combina:

    1. sinais relacionais do PostgreSQL;
    2. features comportamentais vindas do MongoDB;
    3. agregacoes por usuario e item;
    4. similaridade item-item exportada em JSON para apoiar reordenacao kNN;
    5. classificador binario ML.NET para priorizar recomendacoes.

    ## Metricas

    | Modelo | Accuracy | Precision | Recall | F1 | ROC AUC |
    |---|---:|---:|---:|---:|---:|
    | ML.NET incrementado | {{Percent(report.ModeloIncrementadoMlNet.Accuracy)}} | {{Percent(report.ModeloIncrementadoMlNet.Precision)}} | {{Percent(report.ModeloIncrementadoMlNet.Recall)}} | {{Percent(report.ModeloIncrementadoMlNet.F1)}} | {{Percent(report.ModeloIncrementadoMlNet.RocAuc)}} |

    ## Top-K

    | Metrica | Valor |
    |---|---:|
    | Precision@10 | {{Percent(report.TopK.PrecisionAt10)}} |
    | Recall@10 | {{Percent(report.TopK.RecallAt10)}} |
    | Hit Rate@10 | {{Percent(report.TopK.HitRateAt10)}} |

    ## Limitacoes

    - O treino ainda usa exports offline do PostgreSQL e MongoDB.
    - O dataset historico usa usuario numerico; usuarios UUID da API precisam de mapeamento na integracao.
    - O `score_implicito` segue altamente correlacionado com `rating`, entao as metricas de classificacao devem ser lidas como validacao funcional do pipeline, nao como comparacao direta com RMSE/MAE do Funk SVD.
    - A similaridade item-item exportada e uma aproximacao simples baseada em agregados; pode ser substituida por kNN sobre embeddings em iteracoes futuras.

    ## Artefatos

    - Modelo ML.NET: `{{report.Artefatos.Modelo}}`
    - Similaridades item-item: `{{report.Artefatos.Similaridades}}`
    - Metricas JSON: `{{report.Artefatos.Metricas}}`
    - Exemplos CSV: `{{report.Artefatos.Exemplos}}`
    - Projeto de treino: `{{report.Artefatos.Treinamento}}`

    ## Como reproduzir

    ```powershell
    dotnet run --project src/backend/ml-engine/training/Spotlight.Recomendacao.ModelTraining/Spotlight.Recomendacao.ModelTraining.csproj
    ```
    """;

    File.WriteAllText(path, markdown, Encoding.UTF8);
}

static void WriteJson<T>(string path, T value)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    File.WriteAllText(path, JsonSerializer.Serialize(value, options), Encoding.UTF8);
}

static string RelativePath(string path) =>
    Path.GetRelativePath(Directory.GetCurrentDirectory(), path).Replace('\\', '/');

static string Percent(double value) => $"{value * 100:0.00}%";

public sealed record RelationalRow(int UserId, int ItemId, float Rating, float ScoreImplicito);

public sealed record MongoInteraction(int UserId, int ItemId, float Score);

public sealed record AggregateStats(float AverageOrSum, int Count);

public sealed record ItemNeighbor(int ItemId, double Similarity);

public class ModelInput
{
    public float UserId { get; init; }
    public float ItemId { get; init; }
    public bool Label { get; init; }
    public float ScoreImplicito { get; init; }
    public float MongoInteractionScore { get; init; }
    public float MongoInteractionCount { get; init; }
    public float UserRatingMean { get; init; }
    public float UserInteractions { get; init; }
    public float ItemRatingMean { get; init; }
    public float ItemInteractions { get; init; }
    public float ItemSimilarityScore { get; init; }
}

public sealed class ScoredRecommendation : ModelInput
{
    [ColumnName("Probability")]
    public float Probability { get; init; }
}

public sealed class TrainingOptions
{
    public string PostgresCsv { get; init; } =
        Path.GetFullPath("src/dataset/recomendacao_balanceado.csv");

    public string MongoJson { get; init; } =
        Path.GetFullPath("src/dataset/interacoes_mongo_exemplo.jsonl");

    public string ArtifactsDir { get; init; } =
        Path.GetFullPath("src/models/artifacts");

    public string ReportsDir { get; init; } =
        Path.GetFullPath("src/models/reports");

    public static TrainingOptions Parse(string[] args)
    {
        var values = args
            .Chunk(2)
            .Where(chunk => chunk.Length == 2)
            .ToDictionary(chunk => chunk[0], chunk => chunk[1]);

        return new TrainingOptions
        {
            PostgresCsv = Path.GetFullPath(values.GetValueOrDefault("--postgres-csv")
                ?? "src/dataset/recomendacao_balanceado.csv"),
            MongoJson = Path.GetFullPath(values.GetValueOrDefault("--mongo-json")
                ?? "src/dataset/interacoes_mongo_exemplo.jsonl"),
            ArtifactsDir = Path.GetFullPath(values.GetValueOrDefault("--artifacts-dir")
                ?? "src/models/artifacts"),
            ReportsDir = Path.GetFullPath(values.GetValueOrDefault("--reports-dir")
                ?? "src/models/reports")
        };
    }
}

public sealed class MetricsReport
{
    public DatasetReport Dataset { get; init; } = new();
    public FunkSvdReference ReferenciaFunkSvdNotebook { get; init; } = new();
    public ModelMetrics ModeloIncrementadoMlNet { get; init; } = new();
    public TopKMetrics TopK { get; init; } = new();
    public IReadOnlyList<string> Features { get; init; } = [];
    public ArtifactsReport Artefatos { get; init; } = new();
}

public sealed class DatasetReport
{
    public string PostgresCsv { get; init; } = string.Empty;
    public string MongoJson { get; init; } = string.Empty;
    public int Registros { get; init; }
    public int EventosMongoAproveitados { get; init; }
    public double TaxaPositiva { get; init; }
    public int Train { get; init; }
    public int Test { get; init; }
}

public sealed class FunkSvdReference
{
    public double RmseAproximado { get; init; }
    public double MaeAproximado { get; init; }
    public string Fonte { get; init; } = string.Empty;
}

public sealed class ModelMetrics
{
    public double Accuracy { get; init; }
    public double Precision { get; init; }
    public double Recall { get; init; }
    public double F1 { get; init; }
    public double RocAuc { get; init; }
}

public sealed class TopKMetrics
{
    public double PrecisionAt10 { get; init; }
    public double RecallAt10 { get; init; }
    public double HitRateAt10 { get; init; }
}

public sealed class ArtifactsReport
{
    public string Modelo { get; init; } = string.Empty;
    public string Similaridades { get; init; } = string.Empty;
    public string Metricas { get; init; } = string.Empty;
    public string Relatorio { get; init; } = string.Empty;
    public string Exemplos { get; init; } = string.Empty;
    public string Treinamento { get; init; } = string.Empty;
}
