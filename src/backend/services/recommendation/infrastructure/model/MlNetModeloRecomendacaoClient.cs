using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Spotlight.Recomendacao.Application.Services;

namespace Spotlight.Recomendacao.Infrastructure.Model;

public sealed class MlNetModeloRecomendacaoClient(
    IConfiguration configuration,
    ILogger<MlNetModeloRecomendacaoClient> logger) : IModeloRecomendacaoClient
{
    private readonly Lock _lock = new();
    private PredictionEngine<ModelInput, ModelOutput>? _predictionEngine;
    private IReadOnlyList<CandidateRow>? _candidateRows;
    private IReadOnlyDictionary<int, double>? _itemSimilarityScores;

    public bool Habilitado =>
        configuration.GetValue("RecommendationModel:Enabled", false)
        && File.Exists(ModelPath)
        && File.Exists(CandidateCsvPath);

    private string ModelPath =>
        ResolvePath(configuration["RecommendationModel:ModelPath"]
            ?? "../../../models/artifacts/recomendacao_incrementada_mlnet.zip");

    private string CandidateCsvPath =>
        ResolvePath(configuration["RecommendationModel:CandidateCsvPath"]
            ?? "../../../dataset/recomendacao_balanceado.csv");

    private string? SimilaritiesPath
    {
        get
        {
            var path = ResolvePath(configuration["RecommendationModel:SimilaritiesPath"]
                ?? "../../../models/artifacts/item_similarities.json");
            return File.Exists(path) ? path : null;
        }
    }

    public Task<IReadOnlyList<ModeloRecomendacaoPredicao>> PredizerAsync(
        Guid usuarioId,
        int limite,
        CancellationToken cancellationToken)
    {
        if (!Habilitado)
        {
            logger.LogDebug("Modelo ML.NET de recomendacao desabilitado ou artefatos ausentes.");
            return Task.FromResult<IReadOnlyList<ModeloRecomendacaoPredicao>>([]);
        }

        var engine = GetPredictionEngine();
        var candidates = GetCandidateRows();
        var similarityScores = GetItemSimilarityScores();
        var numericUserId = StableNumericUserId(usuarioId);
        var userRows = candidates.Where(row => row.UserId == numericUserId).ToList();

        if (userRows.Count == 0)
        {
            userRows = candidates
                .GroupBy(row => row.ItemId)
                .Select(group => group.MaxBy(row => row.ScoreImplicito)!)
                .ToList();
        }

        var predictions = userRows
            .Select(row =>
            {
                var input = row.ToModelInput(
                    numericUserId,
                    similarityScores.GetValueOrDefault(row.ItemId, row.ScoreImplicito));
                var output = engine.Predict(input);
                return new
                {
                    row.ItemId,
                    Score = Math.Clamp(output.Probability, 0.0f, 1.0f),
                    Similarity = input.ItemSimilarityScore
                };
            })
            .OrderByDescending(item => item.Score)
            .Take(limite)
            .Select(item => new ModeloRecomendacaoPredicao(
                item.ItemId,
                Convert.ToDecimal(item.Score),
                "mlnet_hibrido",
                $"Modelo ML.NET com sinais PostgreSQL, MongoDB e kNN; similaridade {item.Similarity:0.###}"))
            .ToList();

        return Task.FromResult<IReadOnlyList<ModeloRecomendacaoPredicao>>(predictions);
    }

    private PredictionEngine<ModelInput, ModelOutput> GetPredictionEngine()
    {
        if (_predictionEngine is not null)
        {
            return _predictionEngine;
        }

        lock (_lock)
        {
            if (_predictionEngine is not null)
            {
                return _predictionEngine;
            }

            var ml = new MLContext(seed: 42);
            var model = ml.Model.Load(ModelPath, out _);
            _predictionEngine = ml.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            return _predictionEngine;
        }
    }

    private IReadOnlyList<CandidateRow> GetCandidateRows()
    {
        if (_candidateRows is not null)
        {
            return _candidateRows;
        }

        lock (_lock)
        {
            if (_candidateRows is not null)
            {
                return _candidateRows;
            }

            var rows = File.ReadLines(CandidateCsvPath)
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(CandidateRow.Parse)
                .ToList();

            var userStats = rows
                .GroupBy(row => row.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => new AggregateStats(group.Average(row => row.Rating), group.Count()));

            var itemStats = rows
                .GroupBy(row => row.ItemId)
                .ToDictionary(
                    group => group.Key,
                    group => new AggregateStats(group.Average(row => row.Rating), group.Count()));

            _candidateRows = rows
                .Select(row => row with
                {
                    UserRatingMean = userStats[row.UserId].Average,
                    UserInteractions = userStats[row.UserId].Count,
                    ItemRatingMean = itemStats[row.ItemId].Average,
                    ItemInteractions = itemStats[row.ItemId].Count
                })
                .ToList();

            return _candidateRows;
        }
    }

    private IReadOnlyDictionary<int, double> GetItemSimilarityScores()
    {
        if (_itemSimilarityScores is not null)
        {
            return _itemSimilarityScores;
        }

        lock (_lock)
        {
            if (_itemSimilarityScores is not null)
            {
                return _itemSimilarityScores;
            }

            if (SimilaritiesPath is null)
            {
                _itemSimilarityScores = new Dictionary<int, double>();
                return _itemSimilarityScores;
            }

            var json = File.ReadAllText(SimilaritiesPath);
            var raw = JsonSerializer.Deserialize<Dictionary<int, List<ItemNeighbor>>>(json)
                ?? new Dictionary<int, List<ItemNeighbor>>();

            _itemSimilarityScores = raw.ToDictionary(
                item => item.Key,
                item => item.Value.Count == 0 ? 0.0 : item.Value.Average(neighbor => neighbor.Similarity));

            return _itemSimilarityScores;
        }
    }

    private static int StableNumericUserId(Guid usuarioId)
    {
        var bytes = usuarioId.ToByteArray();
        var value = BitConverter.ToUInt32(bytes, 0);
        return Convert.ToInt32(value % 10000);
    }

    private static string ResolvePath(string value) =>
        Path.GetFullPath(
            Environment.ExpandEnvironmentVariables(value),
            Directory.GetCurrentDirectory());

    private sealed record AggregateStats(float Average, int Count);

    private sealed record ItemNeighbor(int ItemId, double Similarity);

    private sealed record CandidateRow(
        int UserId,
        int ItemId,
        float Rating,
        float ScoreImplicito,
        float UserRatingMean = 0,
        int UserInteractions = 0,
        float ItemRatingMean = 0,
        int ItemInteractions = 0)
    {
        public static CandidateRow Parse(string line)
        {
            var parts = line.Split(',');
            return new CandidateRow(
                int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture));
        }

        public ModelInput ToModelInput(int userId, double itemSimilarityScore) => new()
        {
            UserId = userId,
            ItemId = ItemId,
            ScoreImplicito = ScoreImplicito,
            MongoInteractionScore = 0,
            MongoInteractionCount = 0,
            UserRatingMean = UserRatingMean,
            UserInteractions = UserInteractions,
            ItemRatingMean = ItemRatingMean,
            ItemInteractions = ItemInteractions,
            ItemSimilarityScore = Convert.ToSingle(itemSimilarityScore)
        };
    }

    public sealed class ModelInput
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

    public sealed class ModelOutput
    {
        [ColumnName("Probability")]
        public float Probability { get; init; }
    }
}
