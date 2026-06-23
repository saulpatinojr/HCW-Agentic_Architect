using WorkspaceManager.Services;

namespace WorkspaceManager.Features.ContextOptimization;

public partial class ContextOptimizationDashboardPage : ContentPage
{
    private readonly ContextOptimizationMetricsService _metricsService;
    private readonly DashboardWindowService _windowService;

    public ContextOptimizationDashboardPage(
        ContextOptimizationMetricsService metricsService,
        DashboardWindowService windowService)
    {
        _metricsService = metricsService;
        _windowService = windowService;

        InitializeComponent();
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

    private void RefreshMetrics()
    {
        var snapshot = _metricsService.GetSnapshot();
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
}
