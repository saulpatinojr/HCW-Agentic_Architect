using System.Text;
using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class ContextOptimizationExportService
{
    public async Task<string> ExportCsvAsync(CompressionSnapshot snapshot)
    {
        string exportsDir = EnsureExportDirectory();
        string filePath = Path.Combine(
            exportsDir,
            $"context-optimization-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");

        var csv = new StringBuilder();
        csv.AppendLine("timestamp_utc,requests_total,tokens_saved,savings_percent,overhead_ms_average,output_tokens_saved,estimated_cost_saved_usd");
        csv.AppendLine(string.Join(",",
            snapshot.Timestamp.ToUniversalTime().ToString("O"),
            snapshot.RequestsTotal,
            snapshot.TokensSaved,
            snapshot.SavingsPercent.ToString("0.##"),
            snapshot.OverheadMsAverage.ToString("0.##"),
            snapshot.OutputTokensSaved,
            snapshot.EstimatedCostSavedUsd.ToString("0.##")));

        csv.AppendLine();
        csv.AppendLine("partner,tokens_saved,percent_of_total");
        foreach (var partner in snapshot.PartnerSavings)
        {
            csv.AppendLine(string.Join(",",
                Escape(partner.Partner),
                partner.TokensSaved,
                partner.PercentOfTotal.ToString("0.##")));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());
        return filePath;
    }

    public async Task<string> ExportBiSchemaAsync()
    {
        string exportsDir = EnsureExportDirectory();
        string filePath = Path.Combine(exportsDir, "context-optimization-schema.json");

        var schema = new
        {
            dataset = "context_optimization",
            version = "1.0.0",
            tables = new object[]
            {
                new
                {
                    name = "snapshot_metrics",
                    description = "Session-level context optimization metrics",
                    primaryKey = new[] { "timestamp_utc" },
                    fields = new object[]
                    {
                        new { name = "timestamp_utc", type = "datetime", nullable = false },
                        new { name = "requests_total", type = "int", nullable = false },
                        new { name = "tokens_saved", type = "int", nullable = false },
                        new { name = "savings_percent", type = "double", nullable = false },
                        new { name = "overhead_ms_average", type = "double", nullable = false },
                        new { name = "output_tokens_saved", type = "int", nullable = false },
                        new { name = "estimated_cost_saved_usd", type = "double", nullable = false }
                    }
                },
                new
                {
                    name = "partner_savings",
                    description = "Per-partner savings for adapter ecosystem monitoring",
                    primaryKey = new[] { "timestamp_utc", "partner" },
                    fields = new object[]
                    {
                        new { name = "timestamp_utc", type = "datetime", nullable = false },
                        new { name = "partner", type = "string", nullable = false },
                        new { name = "tokens_saved", type = "int", nullable = false },
                        new { name = "percent_of_total", type = "double", nullable = false }
                    }
                }
            },
            ingestion = new
            {
                mode = "append",
                recommendedIntervalMinutes = 5,
                sourceFormats = new[] { "csv", "json" }
            },
            grafana = new
            {
                suggestedDatasource = "sqlite/csv",
                keyPanels = new[]
                {
                    "tokens_saved",
                    "savings_percent",
                    "overhead_ms_average",
                    "partner_savings_distribution"
                }
            }
        };

        string json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }

    private static string EnsureExportDirectory()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string exportsDir = Path.Combine(root, "AIArchitectAgents", "dashboard-exports");
        Directory.CreateDirectory(exportsDir);
        return exportsDir;
    }

    private static string Escape(string value)
    {
        if (value.Contains(','))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
