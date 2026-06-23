using System.Diagnostics;
using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class ContextOptimizationMetricsService
{
    private const double HighOverheadThresholdMs = 30;
    private const double LowPartnerSavingsThresholdPct = 3;

    private readonly object _sync = new();
    private readonly ContextOptimizationHistoryStore _historyStore;
    private readonly PartnerAdapterHealthService _healthService;
    private CompressionSnapshot _lastKnownSnapshot = new();
    private string _repoRootPath = string.Empty;

    public ContextOptimizationMetricsService(
        ContextOptimizationHistoryStore historyStore,
        PartnerAdapterHealthService healthService)
    {
        _historyStore = historyStore;
        _healthService = healthService;
    }

    public void SetRepoRootPath(string repoRootPath)
    {
        _repoRootPath = repoRootPath;
    }

    public CompressionSnapshot GetSnapshot()
    {
        if (string.IsNullOrWhiteSpace(_repoRootPath))
        {
            return _lastKnownSnapshot;
        }

        var serverPath = Path.Combine(
            _repoRootPath,
            "workspace-config",
            "mcp-servers",
            "token-compressor",
            "server.py");

        if (!File.Exists(serverPath))
        {
            return _lastKnownSnapshot;
        }

        var output = ExecuteStatsCommand(serverPath);
        if (string.IsNullOrWhiteSpace(output))
        {
            return _lastKnownSnapshot;
        }

        var snapshot = ParseSnapshot(output);
        _historyStore.SaveSnapshot(snapshot);
        snapshot.DayTrend = _historyStore.GetDayTrend();
        snapshot.WeekTrend = _historyStore.GetWeekTrend();
        snapshot.MonthTrend = _historyStore.GetMonthTrend();
        snapshot.PartnerHealth = _healthService.GetHealth();
        snapshot.Alerts = BuildAlerts(snapshot);

        lock (_sync)
        {
            _lastKnownSnapshot = snapshot;
            return _lastKnownSnapshot;
        }
    }

    private static string ExecuteStatsCommand(string serverPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{serverPath}\" --stats-json",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (process.ExitCode != 0)
            {
                return string.Empty;
            }

            return output;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static CompressionSnapshot ParseSnapshot(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var requests = ReadInt(root, "requests", "total");
        var tokensSaved = ReadInt(root, "tokens", "saved");
        var savingsPercent = ReadDouble(root, "tokens", "savings_percent");
        var outputTokensSaved = ReadInt(root, "tokens", "output_saved");
        var overheadMs = ReadDouble(root, "overhead", "average_ms");

        var partnerSavings = ReadPartnerSavings(root, tokensSaved);
        var history = ReadHistory(root);

        var ttlBuckets = new List<ObservedTtlBucket>
        {
            new() { Label = "5m", WriteMixPercent = 44.0 },
            new() { Label = "1h", WriteMixPercent = 56.0 }
        };

        return new CompressionSnapshot
        {
            Timestamp = DateTime.UtcNow,
            RequestsTotal = requests,
            TokensSaved = tokensSaved,
            SavingsPercent = savingsPercent,
            EstimatedCostSavedUsd = Math.Round(tokensSaved * 0.000002, 2),
            OverheadMsAverage = overheadMs,
            OutputTokensSaved = outputTokensSaved,
            PartnerSavings = partnerSavings,
            ObservedTtlBuckets = ttlBuckets,
            History = history
        };
    }

    private static IReadOnlyList<CompressionAlertItem> BuildAlerts(
        CompressionSnapshot snapshot)
    {
        var alerts = new List<CompressionAlertItem>();

        if (snapshot.OverheadMsAverage > HighOverheadThresholdMs)
        {
            alerts.Add(new CompressionAlertItem
            {
                Severity = "Warning",
                Message = "High optimization overhead",
                Detail =
                    $"Average overhead is {snapshot.OverheadMsAverage:N1} ms (threshold {HighOverheadThresholdMs:N0} ms)."
            });
        }

        foreach (var partner in snapshot.PartnerSavings)
        {
            if (partner.PercentOfTotal >= LowPartnerSavingsThresholdPct)
            {
                continue;
            }

            alerts.Add(new CompressionAlertItem
            {
                Severity = "Info",
                Message = $"Low savings contribution for {partner.Partner}",
                Detail =
                    $"{partner.PercentOfTotal:N1}% of total savings. Consider partner-specific compression profile tuning."
            });
        }

        if (alerts.Count == 0)
        {
            alerts.Add(new CompressionAlertItem
            {
                Severity = "Info",
                Message = "No active threshold alerts",
                Detail = "Overhead and partner savings are within configured ranges."
            });
        }

        return alerts;
    }

    private static int ReadInt(JsonElement root, string section, string key)
    {
        if (!root.TryGetProperty(section, out var sectionNode))
        {
            return 0;
        }

        if (!sectionNode.TryGetProperty(key, out var valueNode))
        {
            return 0;
        }

        return valueNode.ValueKind == JsonValueKind.Number && valueNode.TryGetInt32(out int value)
            ? value
            : 0;
    }

    private static double ReadDouble(JsonElement root, string section, string key)
    {
        if (!root.TryGetProperty(section, out var sectionNode))
        {
            return 0;
        }

        if (!sectionNode.TryGetProperty(key, out var valueNode))
        {
            return 0;
        }

        return valueNode.ValueKind == JsonValueKind.Number && valueNode.TryGetDouble(out double value)
            ? value
            : 0;
    }

    private static IReadOnlyList<PartnerSavingsItem> ReadPartnerSavings(JsonElement root, int totalSaved)
    {
        var items = new List<PartnerSavingsItem>();
        if (!root.TryGetProperty("projects", out var projectsNode) || projectsNode.ValueKind != JsonValueKind.Object)
        {
            return items;
        }

        var safeTotal = Math.Max(1, totalSaved);
        foreach (var property in projectsNode.EnumerateObject())
        {
            if (!property.Value.TryGetInt32(out int value))
            {
                continue;
            }

            items.Add(new PartnerSavingsItem
            {
                Partner = property.Name,
                TokensSaved = value,
                PercentOfTotal = Math.Round((double)value / safeTotal * 100, 1)
            });
        }

        return items.OrderByDescending(item => item.TokensSaved).ToList();
    }

    private static IReadOnlyList<CompressionHistoryPoint> ReadHistory(JsonElement root)
    {
        var history = new List<CompressionHistoryPoint>();
        if (!root.TryGetProperty("history", out var historyNode) || historyNode.ValueKind != JsonValueKind.Array)
        {
            return history;
        }

        foreach (var item in historyNode.EnumerateArray())
        {
            if (!item.TryGetProperty("timestamp_utc", out var timestampNode))
            {
                continue;
            }

            if (!item.TryGetProperty("tokens_saved", out var savedNode) || !savedNode.TryGetInt32(out int saved))
            {
                continue;
            }

            if (!DateTime.TryParse(timestampNode.GetString(), out DateTime timestamp))
            {
                timestamp = DateTime.UtcNow;
            }

            history.Add(new CompressionHistoryPoint
            {
                Timestamp = timestamp,
                TokensSaved = saved
            });
        }

        return history.TakeLast(30).ToList();
    }
}
