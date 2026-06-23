namespace WorkspaceManager.Services;

public sealed class ContextOptimizationMetricsService
{
    private static readonly string[] Partners =
    [
        "codex",
        "claude",
        "github",
        "copilot",
        "antigravity",
        "vscode"
    ];

    private readonly Random _random = new(42);
    private readonly Dictionary<string, int> _partnerSavings = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<CompressionHistoryPoint> _history = [];
    private int _requests;
    private int _tokensSaved;

    public ContextOptimizationMetricsService()
    {
        foreach (var partner in Partners)
        {
            _partnerSavings[partner] = 0;
        }
    }

    public CompressionSnapshot GetSnapshot()
    {
        SimulateTick();

        var total = Math.Max(1, _tokensSaved);
        var partnerSavings = _partnerSavings
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new PartnerSavingsItem
            {
                Partner = pair.Key,
                TokensSaved = pair.Value,
                PercentOfTotal = Math.Round((double)pair.Value / total * 100, 1)
            })
            .ToList();

        var ttlBuckets = new List<ObservedTtlBucket>
        {
            new() { Label = "5m", WriteMixPercent = 44.0 },
            new() { Label = "1h", WriteMixPercent = 56.0 }
        };

        var savingsPercent = Math.Min(96, 45 + (_tokensSaved % 50) / 2.0);

        return new CompressionSnapshot
        {
            Timestamp = DateTime.UtcNow,
            RequestsTotal = _requests,
            TokensSaved = _tokensSaved,
            SavingsPercent = Math.Round(savingsPercent, 1),
            EstimatedCostSavedUsd = Math.Round(_tokensSaved * 0.000002, 2),
            OverheadMsAverage = Math.Round(2 + (_requests % 7) * 0.7, 1),
            OutputTokensSaved = (int)(_tokensSaved * 0.22),
            PartnerSavings = partnerSavings,
            ObservedTtlBuckets = ttlBuckets,
            History = _history.ToList()
        };
    }

    private void SimulateTick()
    {
        _requests += _random.Next(4, 11);
        var newSavings = _random.Next(400, 1500);
        _tokensSaved += newSavings;

        var partner = Partners[_random.Next(0, Partners.Length)];
        _partnerSavings[partner] += newSavings;

        _history.Add(new CompressionHistoryPoint
        {
            Timestamp = DateTime.UtcNow,
            TokensSaved = _tokensSaved
        });

        if (_history.Count > 30)
        {
            _history.RemoveAt(0);
        }
    }
}
