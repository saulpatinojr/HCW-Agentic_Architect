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
    public IReadOnlyList<CompressionAlertItem> Alerts { get; set; } = [];
    public IReadOnlyList<PartnerAdapterHealthItem> PartnerHealth { get; set; } = [];
    public IReadOnlyList<CompressionTrendPoint> DayTrend { get; set; } = [];
    public IReadOnlyList<CompressionTrendPoint> WeekTrend { get; set; } = [];
    public IReadOnlyList<CompressionTrendPoint> MonthTrend { get; set; } = [];
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

public sealed class CompressionTrendPoint
{
    public string Label { get; set; } = string.Empty;
    public int TokensSaved { get; set; }
}

public sealed class CompressionAlertItem
{
    public string Severity { get; set; } = "Info";
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string AccentColor => Severity switch
    {
        "Critical" => "#B91C1C",
        "Warning" => "#B45309",
        _ => "#0F766E"
    };
}

public sealed class PartnerAdapterHealthItem
{
    public string Partner { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public string Detail { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string AccentColor => IsHealthy ? "#166534" : "#B91C1C";
}
