using WorkspaceManager.Services;
using System.Text.Json;

namespace WorkspaceManager.Features.ContextOptimization;

public partial class ContextOptimizationDashboardPage : ContentPage
{
    private readonly ContextOptimizationMetricsService _metricsService;
    private readonly DashboardWindowService _windowService;
    private readonly IDispatcherTimer _refreshTimer;
    private CompressionSnapshot _lastSnapshot = new();
    private bool _autoRefresh = true;

    public ContextOptimizationDashboardPage(
        ContextOptimizationMetricsService metricsService,
        DashboardWindowService windowService)
    {
        _metricsService = metricsService;
        _windowService = windowService;

        InitializeComponent();
        _refreshTimer = Dispatcher.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(4);
        _refreshTimer.Tick += (_, _) => RefreshMetrics();
        _refreshTimer.Start();
        RefreshMetrics();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        RefreshMetrics();
    }

    private void OnDetachClicked(object sender, EventArgs e)
    {
        var detachedPage = new ContextOptimizationDashboardPage(_metricsService, _windowService);
        _windowService.OpenDashboardWindow(detachedPage);
    }

    private void OnAutoRefreshClicked(object sender, EventArgs e)
    {
        _autoRefresh = !_autoRefresh;
        AutoRefreshButton.Text = _autoRefresh ? "Auto refresh: on" : "Auto refresh: off";

        if (_autoRefresh)
        {
            _refreshTimer.Start();
            RefreshMetrics();
        }
        else
        {
            _refreshTimer.Stop();
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string exportsDir = Path.Combine(root, "AIArchitectAgents", "dashboard-exports");
            Directory.CreateDirectory(exportsDir);

            string filePath = Path.Combine(
                exportsDir,
                $"context-optimization-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");

            var json = JsonSerializer.Serialize(
                _lastSnapshot,
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);
            await DisplayAlert("Export complete", $"Saved: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export failed", ex.Message, "OK");
        }
    }

    private void RefreshMetrics()
    {
        var snapshot = _metricsService.GetSnapshot();
        _lastSnapshot = snapshot;
        TokensSavedLabel.Text = snapshot.TokensSaved.ToString("N0");
        SavingsPercentLabel.Text = $"{snapshot.SavingsPercent:N1}%";
        CostSavedLabel.Text = $"${snapshot.EstimatedCostSavedUsd:N2}";
        OverheadLabel.Text = $"{snapshot.OverheadMsAverage:N1} ms";
        OutputSavingsLabel.Text = $"Output tokens saved: {snapshot.OutputTokensSaved:N0}";

        PartnerSavingsCollectionView.ItemsSource = snapshot.PartnerSavings;
        TtlBucketsCollectionView.ItemsSource = snapshot.ObservedTtlBuckets;

        var trendWindow = snapshot.History.TakeLast(5).ToList();
        if (trendWindow.Count >= 2)
        {
            var delta = trendWindow[^1].TokensSaved - trendWindow[0].TokensSaved;
            TrendLabel.Text = $"Last {trendWindow.Count} ticks: +{delta:N0} optimized tokens";
        }
        else
        {
            TrendLabel.Text = "Waiting for additional trend data.";
        }

        UpdatedLabel.Text = $"Updated: {snapshot.Timestamp.ToLocalTime():g}";
    }

    protected override void OnDisappearing()
    {
        _refreshTimer.Stop();
        base.OnDisappearing();
    }
}
