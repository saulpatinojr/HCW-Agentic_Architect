namespace WorkspaceManager;

public sealed class CompressionSnapshot
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int RequestsTotal { get; set; }
    public int TokensSaved { get; set; }
    public double SavingsPercent { get; set; }
    public double EstimatedCostSavedUsd { get; set; }
    public double OverheadMsAverage { get; set; }
    public int OutputTokensSaved { get; set; }
    public IReadOnlyList<PartnerSavingsItem> PartnerSavings { get; set; } = [];
    public IReadOnlyList<ObservedTtlBucket> ObservedTtlBuckets { get; set; } = [];
    public IReadOnlyList<CompressionHistoryPoint> History { get; set; } = [];
}

public sealed class PartnerSavingsItem
{
    public string Partner { get; set; } = string.Empty;
    public int TokensSaved { get; set; }
    public double PercentOfTotal { get; set; }
}

public sealed class ObservedTtlBucket
{
    public string Label { get; set; } = string.Empty;
    public double WriteMixPercent { get; set; }
}

public sealed class CompressionHistoryPoint
{
    public DateTime Timestamp { get; set; }
    public int TokensSaved { get; set; }
}
