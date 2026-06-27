using System.Collections.ObjectModel;
using System.Diagnostics;
using WorkspaceManager.Services;
using WorkspaceManager.Features.ContextOptimization;

namespace WorkspaceManager;

public partial class MainPage : ContentPage
{
    private readonly AppPreferenceStore _appPreferenceStore = new();
    private readonly WorkspaceCatalogService _workspaceCatalogService;
    private readonly WorkspaceSystemCheckService _workspaceSystemCheckService;
    private readonly WorkspaceActivationService _workspaceActivationService;
    private readonly WorkspacePackUpdateService _workspacePackUpdateService;
    private readonly ProviderRegistryService _providerRegistryService;
    private readonly WorkspaceFolderService _workspaceFolderService;
    private readonly HelperMcpHealthService _helperMcpHealthService;
    private readonly ContextOptimizationMetricsService _contextOptimizationMetricsService;
    private readonly OptionalFeatureSetupService _optionalFeatureSetupService;
    private readonly DashboardWindowService _dashboardWindowService;
    private readonly ContextOptimizationExportService _contextOptimizationExportService;
    private readonly ContextOptimizationDashboardPage _contextOptimizationDashboardPage;
    private string _repoRootPath = string.Empty;
    private AgentViewModel? _selectedPack;
    private bool _isCompactLayout;
    private bool _previewModeByDefault = true;
    private bool _includeHelperMcp = true;
    private OptionalFeaturePromptState _optionalTrackState = new();

    public ObservableCollection<AgentViewModel> DiscoveredAgents { get; } = [];
    public ObservableCollection<ActivityLogEntry> ActivityEntries { get; } = [];
    public ObservableCollection<ProviderGroup> Providers { get; } = [];
    public ObservableCollection<ProviderInfo> SelectedProviders { get; } = [];
    public ObservableCollection<WorkspaceLink> PackLinks { get; } = [];
    public ObservableCollection<WorkspaceFolderNode> FolderNodes { get; } = [];
    public ObservableCollection<ManifestToolRequirement> SelectedRequiredTools { get; } = [];
    public ObservableCollection<ManifestMcpServerRequirement> SelectedMcpServers { get; } = [];

    public MainPage(
        WorkspaceCatalogService workspaceCatalogService,
        WorkspaceSystemCheckService workspaceSystemCheckService,
        WorkspaceActivationService workspaceActivationService,
        WorkspacePackUpdateService workspacePackUpdateService,
        ProviderRegistryService providerRegistryService,
        WorkspaceFolderService workspaceFolderService,
        HelperMcpHealthService helperMcpHealthService,
        ContextOptimizationMetricsService contextOptimizationMetricsService,
        OptionalFeatureSetupService optionalFeatureSetupService,
        DashboardWindowService dashboardWindowService,
        ContextOptimizationExportService contextOptimizationExportService,
        ContextOptimizationDashboardPage contextOptimizationDashboardPage)
    {
        _workspaceCatalogService = workspaceCatalogService;
        _workspaceSystemCheckService = workspaceSystemCheckService;
        _workspaceActivationService = workspaceActivationService;
        _workspacePackUpdateService = workspacePackUpdateService;
        _providerRegistryService = providerRegistryService;
        _workspaceFolderService = workspaceFolderService;
        _helperMcpHealthService = helperMcpHealthService;
        _contextOptimizationMetricsService = contextOptimizationMetricsService;
        _optionalFeatureSetupService = optionalFeatureSetupService;
        _dashboardWindowService = dashboardWindowService;
        _contextOptimizationExportService = contextOptimizationExportService;
        _contextOptimizationDashboardPage = contextOptimizationDashboardPage;

        InitializeComponent();
        BindCollections();
        LoadUiPreferences();
        DetermineRepositoryRoot();
        LoadProviders();
        RefreshFolderNodes();
        UpdateSummaryState("Ready");
        RefreshDashboardPreview();
        _ = InitializeOptionalTracksAsync();
        _ = RefreshHelperHealthAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUiPreferences();
        _ = RefreshHelperHealthAsync();
    }

    private void LoadUiPreferences()
    {
        var preferences = _appPreferenceStore.Load();
        _previewModeByDefault = preferences.PreviewModeDefault;
        _includeHelperMcp = preferences.EnableHelperMcp;
    }

    private async Task InitializeOptionalTracksAsync()
    {
        _optionalTrackState = await _optionalFeatureSetupService.EvaluateAsync(_repoRootPath);
        ApplyOptionalTrackState(_optionalTrackState);
    }

    private void ApplyOptionalTrackState(OptionalFeaturePromptState state)
    {
        OptionalSetupBanner.IsVisible = state.ShouldPrompt;
        NeverAskAgainCheckBox.IsChecked = state.NeverAskAgain;

        var dashboard = state.Tracks.FirstOrDefault(t => t.Key == "dashboard");
        var sqlite = state.Tracks.FirstOrDefault(t => t.Key == "sqlite");
        var mcp = state.Tracks.FirstOrDefault(t => t.Key == "mcp");

        if (dashboard is not null)
        {
            DashboardTrackCheckBox.IsChecked = dashboard.IsSelected;
            DashboardTrackStatusLabel.Text = dashboard.Detail;
        }

        if (sqlite is not null)
        {
            SqliteTrackCheckBox.IsChecked = sqlite.IsSelected;
            SqliteTrackStatusLabel.Text = sqlite.Detail;
        }

        if (mcp is not null)
        {
            McpTrackCheckBox.IsChecked = mcp.IsSelected;
            McpTrackStatusLabel.Text = mcp.Detail;
        }
    }

    private void RefreshDashboardPreview()
    {
        var snapshot = _contextOptimizationMetricsService.GetSnapshot();
        DashboardPreviewSavedLabel.Text = $"Tokens saved: {snapshot.TokensSaved:N0}";
        DashboardPreviewEfficiencyLabel.Text = $"Compression: {snapshot.SavingsPercent:N1}%";

        var topPartner = snapshot.PartnerSavings.FirstOrDefault();
        DashboardPreviewPartnerLabel.Text = topPartner is null
            ? "Top: n/a"
            : $"Top: {topPartner.Partner} ({topPartner.PercentOfTotal:N1}%)";
    }

    private void BindCollections()
    {
        AgentsCollectionView.ItemsSource = DiscoveredAgents;
        ActivityCollectionView.ItemsSource = ActivityEntries;
        ProvidersCollectionView.ItemsSource = Providers;
        SelectedProvidersCollectionView.ItemsSource = SelectedProviders;
        PackLinksCollectionView.ItemsSource = PackLinks;
        FolderNodesCollectionView.ItemsSource = FolderNodes;
        RequiredToolsCollectionView.ItemsSource = SelectedRequiredTools;
        McpServersCollectionView.ItemsSource = SelectedMcpServers;
    }

    private void LoadProviders()
    {
        Providers.Clear();
        foreach (var providerGroup in _providerRegistryService.GetGroups())
        {
            Providers.Add(providerGroup);
        }
    }

    private void Log(string message, string category = "Workspace", string? detail = null)
    {
        var severity = message.Contains("[X]", StringComparison.OrdinalIgnoreCase) || message.StartsWith("[-]", StringComparison.Ordinal)
            ? "Error"
            : message.Contains("[!]", StringComparison.OrdinalIgnoreCase) || message.Contains("[WARNING]", StringComparison.OrdinalIgnoreCase)
                ? "Warning"
                : message.Contains("[+]", StringComparison.OrdinalIgnoreCase) || message.Contains("[OK]", StringComparison.OrdinalIgnoreCase) || message.Contains("[SYSTEM READY]", StringComparison.OrdinalIgnoreCase)
                    ? "Success"
                    : "Info";

        var entry = new ActivityLogEntry
        {
            Severity = severity,
            Category = category,
            Message = CleanLogMessage(message),
            Detail = detail ?? ExtractDetail(message)
        };

        ActivityEntries.Insert(0, entry);
        LastScanLabel.Text = $"Last update: {DateTime.Now:t}";
    }

    private static string CleanLogMessage(string message)
    {
        return message
            .Replace("[✓]", string.Empty, StringComparison.Ordinal)
            .Replace("[X]", string.Empty, StringComparison.Ordinal)
            .Replace("[x]", string.Empty, StringComparison.Ordinal)
            .Replace("[*]", string.Empty, StringComparison.Ordinal)
            .Replace("[+]", string.Empty, StringComparison.Ordinal)
            .Replace("[-]", string.Empty, StringComparison.Ordinal)
            .Replace("[!]", string.Empty, StringComparison.Ordinal)
            .Trim(' ', '.', '-');
    }

    private static string ExtractDetail(string message)
    {
        int index = message.IndexOf(':', StringComparison.Ordinal);
        return index >= 0 && index + 1 < message.Length ? message[(index + 1)..].Trim() : string.Empty;
    }

    private void DetermineRepositoryRoot()
    {
        _repoRootPath = _workspaceCatalogService.DetermineRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
        _contextOptimizationMetricsService.SetRepoRootPath(_repoRootPath);
        WorkspacePathLabel.Text = $"Workspace: {Path.GetFileName(_repoRootPath)}";
        Log($"Bound to {_repoRootPath}", "Startup", _repoRootPath);
    }

    private void UpdateSummaryState(string status)
    {
        StatusLabel.Text = status;
        SetStatusIndicator(status);
        PackCountLabel.Text = DiscoveredAgents.Count.ToString();
        SelectionCountLabel.Text = DiscoveredAgents.Count(agent => agent.IsSelected).ToString();
        ApplyButton.IsEnabled = DiscoveredAgents.Any(agent => agent.IsSelected);
        UpdateButton.IsEnabled = _selectedPack is not null
            && !string.Equals(_selectedPack.UpdateState, "Current", StringComparison.OrdinalIgnoreCase);
    }

    private void SetStatusIndicator(string status)
    {
        string normalized = status.ToLowerInvariant();

        string color = normalized switch
        {
            var s when s.Contains("fail") || s.Contains("issue") || s.Contains("error") || s.Contains("selection required") => "#D13438",
            var s when s.Contains("update") || s.Contains("checking") || s.Contains("config") || s.Contains("download") => "#FFB900",
            var s when s.Contains("applying") || s.Contains("preview") || s.Contains("scan") || s.Contains("loading") => "#107C10",
            _ => "#0078D4"
        };

        StatusIndicatorDot.BackgroundColor = Color.FromArgb(color);
    }

    private async Task RefreshHelperHealthAsync()
    {
        var health = await _helperMcpHealthService.CheckAsync(_repoRootPath, _includeHelperMcp);
        McpHealthLabel.Text = health.Status;
        McpHealthLabel.TextColor = Color.FromArgb(health.Status == "Helper ready" ? "#107C10" : "#D83B01");
        McpSummaryLabel.Text = health.Summary;

        if (!health.ScriptExists)
        {
            Log("Helper MCP script is missing.", "MCP", health.ScriptPath);
        }
        else if (!health.PythonAvailable)
        {
            Log("Helper MCP script exists, but Python was not found on PATH.", "MCP", health.ScriptPath);
        }
        else
        {
            Log("Helper MCP health check passed.", "MCP", health.Summary);
        }
    }

    private void RefreshFolderNodes()
    {
        FolderNodes.Clear();
        foreach (var node in _workspaceFolderService.BuildNodes(_repoRootPath, _selectedPack))
        {
            FolderNodes.Add(node);
        }
    }

    private void RefreshPackInspector(AgentViewModel? agent)
    {
        _selectedPack = agent;
        SelectedProviders.Clear();
        PackLinks.Clear();
        SelectedRequiredTools.Clear();
        SelectedMcpServers.Clear();

        if (agent is null)
        {
            SelectedPackNameLabel.Text = "Select a pack";
            SelectedPackDescriptionLabel.Text = "Pack metadata, provider links, files, and MCP requirements appear here.";
            UpdateButton.IsEnabled = false;
            RefreshFolderNodes();
            return;
        }

        SelectedPackNameLabel.Text = $"{agent.FriendlyName}  v{agent.Version}";
        SelectedPackDescriptionLabel.Text = $"{agent.Category} | {agent.UpdateState}\n{agent.Description}";

        foreach (var provider in _providerRegistryService.Resolve(agent.ProviderIds))
        {
            SelectedProviders.Add(provider);
        }

        foreach (var link in agent.OfficialLinks.Concat(agent.BestPracticeLinks))
        {
            PackLinks.Add(link);
        }

        foreach (var tool in agent.RequiredTools)
        {
            SelectedRequiredTools.Add(tool);
        }

        foreach (var mcp in agent.RequiredMcpServers)
        {
            SelectedMcpServers.Add(mcp);
        }

        UpdateButton.IsEnabled = !string.Equals(agent.UpdateState, "Current", StringComparison.OrdinalIgnoreCase);
        RefreshFolderNodes();
    }

    private void OnScanWorkspaceClicked(object sender, EventArgs e)
    {
        DiscoveredAgents.Clear();
        foreach (var agent in _workspaceCatalogService.DiscoverAgents(_repoRootPath))
        {
            DiscoveredAgents.Add(agent);
        }

        if (DiscoveredAgents.Count > 0)
        {
            AgentsCollectionView.SelectedItem = DiscoveredAgents[0];
            RefreshPackInspector(DiscoveredAgents[0]);
        }
        else
        {
            RefreshPackInspector(null);
        }

        UpdateSummaryState("Workspace scanned");
        Log($"Discovered {DiscoveredAgents.Count} pack(s).", "Scan");
    }

    private async void OnImportWorkspacePackClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a workspace pack (.zip)"
            });

            if (result != null && result.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string packName = _workspaceCatalogService.ImportAgentPack(_repoRootPath, result.FullPath);
                Log($"Imported workspace pack {packName}.", "Import", result.FullPath);
                OnScanWorkspaceClicked(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            UpdateSummaryState("Import failed");
            Log($"Import failed: {ex.Message}", "Import");
        }
    }

    private async void OnPreviewWorkspaceClicked(object sender, EventArgs e)
    {
        await ActivateWorkspaceAsync(forceDryRun: true);
    }

    private async void OnActivateWorkspaceClicked(object sender, EventArgs e)
    {
        await ActivateWorkspaceAsync(forceDryRun: null);
    }

    private async Task ActivateWorkspaceAsync(bool? forceDryRun)
    {
        var selected = DiscoveredAgents.Where(a => a.IsSelected).ToList();
        SelectionCountLabel.Text = selected.Count.ToString();
        if (!selected.Any())
        {
            UpdateSummaryState("Selection required");
            Log("Select at least one pack before applying changes.", "Activate");
            return;
        }

        bool dryRun = forceDryRun ?? _previewModeByDefault;

        try
        {
            UpdateSummaryState(dryRun ? "Previewing changes" : "Applying selection");
            var result = await _workspaceActivationService.ActivateAsync(new WorkspaceActivationRequest
            {
                RepoRootPath = _repoRootPath,
                SelectedAgents = selected,
                IsDryRun = dryRun,
                IncludeHelperMcp = _includeHelperMcp
            });

            foreach (var line in result.Logs)
            {
                Log(line, dryRun ? "Preview" : "Apply");
            }

            UpdateSummaryState(result.Succeeded ? (dryRun ? "Preview complete" : "Selection applied") : "Action completed with issues");
            RefreshFolderNodes();
            await RefreshHelperHealthAsync();
        }
        catch (Exception ex)
        {
            UpdateSummaryState("Apply failed");
            Log($"Apply failed: {ex.Message}", "Apply");
        }
    }

    private async void OnSystemCheckClicked(object sender, EventArgs e)
    {
        UpdateSummaryState("Checking system");
        var lines = await _workspaceSystemCheckService.RunAsync(_repoRootPath, DiscoveredAgents);
        foreach (var line in lines)
        {
            Log(line, "Preflight");
        }

        int missingCount = lines.Count(line => line.Contains("[X]", StringComparison.Ordinal) || line.Contains("[WARNING]", StringComparison.Ordinal));
        MissingRequirementsLabel.Text = missingCount.ToString();
        UpdateSummaryState("System check complete");
        await RefreshHelperHealthAsync();
    }

    private void OnCheckUpdatesClicked(object sender, EventArgs e)
    {
        OnScanWorkspaceClicked(sender, e);
        Log("Checked catalog states for local packs.", "Update");
    }

    private async void OnUpdateSelectedPackClicked(object sender, EventArgs e)
    {
        if (_selectedPack is null)
        {
            Log("Select a pack before checking for updates.", "Update");
            return;
        }

        if (string.Equals(_selectedPack.UpdateState, "Current", StringComparison.OrdinalIgnoreCase))
        {
            Log($"{_selectedPack.FriendlyName} is already current.", "Update");
            return;
        }

        try
        {
            UpdateSummaryState("Updating pack");
            var result = await _workspacePackUpdateService.UpdateAsync(_repoRootPath, _selectedPack);
            foreach (var line in result.Logs)
            {
                Log(line, "Update");
            }

            if (result.Succeeded)
            {
                Log($"Pack update complete: {result.UpdatedVersion}.", "Update");
                OnScanWorkspaceClicked(sender, e);
            }
            else
            {
                UpdateSummaryState("Update failed");
            }
        }
        catch (Exception ex)
        {
            UpdateSummaryState("Update failed");
            Log($"Update failed: {ex.Message}", "Update");
        }
    }

    private void OnAgentCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        UpdateSummaryState("Selection updated");
    }

    private void OnPackSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshPackInspector(e.CurrentSelection.FirstOrDefault() as AgentViewModel);
    }

    private async void OnFolderNodeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not WorkspaceFolderNode node)
        {
            return;
        }

        FolderNodesCollectionView.SelectedItem = null;
        if (!node.Exists)
        {
            Log($"{node.Label} is not present yet.", "Files", node.Path);
            return;
        }

        try
        {
            OpenLocalPath(node.Path);
            Log($"Opened {node.Label}.", "Files", node.Path);
        }
        catch (Exception ex)
        {
            Log($"Unable to open {node.Label}: {ex.Message}", "Files", node.Path);
        }

        await Task.CompletedTask;
    }

    private async void OnPackLinkClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: string url } || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            await Launcher.Default.OpenAsync(url);
            Log("Opened guidance link.", "Links", url);
        }
        catch (Exception ex)
        {
            Log($"Unable to open guidance link: {ex.Message}", "Links", url);
        }
    }

    private async void OnProviderSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not ProviderInfo provider || string.IsNullOrWhiteSpace(provider.OfficialUrl))
        {
            return;
        }

        try
        {
            await Launcher.Default.OpenAsync(provider.OfficialUrl);
            Log($"Opened {provider.Name}.", "Provider", provider.OfficialUrl);
        }
        catch (Exception ex)
        {
            Log($"Unable to open {provider.Name}: {ex.Message}", "Provider", provider.OfficialUrl);
        }
    }

    private void OnRefreshDashboardClicked(object sender, EventArgs e)
    {
        RefreshDashboardPreview();
        Log("Context optimization preview refreshed.", "Dashboard");
    }

    private async void OnOpenDashboardClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(_contextOptimizationDashboardPage);
    }

    private async void OnOpenSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new SettingsPage());
        Log("Opened settings.", "Settings");
    }
    private void OnDetachDashboardClicked(object sender, EventArgs e)
    {
        var detachedPage = new ContextOptimizationDashboardPage(
            _contextOptimizationMetricsService,
            _dashboardWindowService,
            _contextOptimizationExportService);
        _dashboardWindowService.OpenDashboardWindow(detachedPage);
        Log("Opened detached context optimization dashboard.", "Dashboard");
    }

    private async void OnInstallOptionalTracksClicked(object sender, EventArgs e)
    {
        var request = new OptionalFeatureInstallRequest
        {
            InstallDashboard = DashboardTrackCheckBox.IsChecked,
            InstallSqlite = SqliteTrackCheckBox.IsChecked,
            InstallMcp = McpTrackCheckBox.IsChecked,
            NeverAskAgain = NeverAskAgainCheckBox.IsChecked
        };

        var result = await _optionalFeatureSetupService.InstallAsync(_repoRootPath, request);
        foreach (var line in result.Logs)
        {
            Log(line, "Optional setup");
        }

        _optionalTrackState = result.State;
        ApplyOptionalTrackState(_optionalTrackState);

        if (result.Succeeded)
        {
            Log("Optional track setup completed.", "Optional setup");
            await RefreshHelperHealthAsync();
        }
        else
        {
            Log("Optional track setup completed with warnings.", "Optional setup");
        }
    }

    private async void OnSkipOptionalTracksClicked(object sender, EventArgs e)
    {
        await _optionalFeatureSetupService.SetNeverAskAgainAsync(NeverAskAgainCheckBox.IsChecked);
        OptionalSetupBanner.IsVisible = false;
        Log(
            NeverAskAgainCheckBox.IsChecked
                ? "Optional setup prompt disabled."
                : "Skipped optional setup for this run.",
            "Optional setup");
    }

    private static void OpenLocalPath(string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        bool shouldUseCompact = Width < 1060;
        if (shouldUseCompact == _isCompactLayout)
        {
            return;
        }

        _isCompactLayout = shouldUseCompact;
        if (_isCompactLayout)
        {
            MainContentGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new() { Width = GridLength.Star }
            };
            MainContentGrid.RowDefinitions = new RowDefinitionCollection
            {
                new() { Height = GridLength.Auto },
                new() { Height = GridLength.Auto },
                new() { Height = GridLength.Auto }
            };

            Grid.SetColumn(LeftRail, 0);
            Grid.SetRow(LeftRail, 0);
            Grid.SetColumn(CenterPanel, 0);
            Grid.SetRow(CenterPanel, 1);
            Grid.SetColumn(InspectorPanel, 0);
            Grid.SetRow(InspectorPanel, 2);
            return;
        }

        MainContentGrid.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new() { Width = new GridLength(300) },
            new() { Width = GridLength.Star },
            new() { Width = new GridLength(330) }
        };
        MainContentGrid.RowDefinitions = new RowDefinitionCollection
        {
            new() { Height = GridLength.Auto }
        };

        Grid.SetColumn(LeftRail, 0);
        Grid.SetRow(LeftRail, 0);
        Grid.SetColumn(CenterPanel, 1);
        Grid.SetRow(CenterPanel, 0);
        Grid.SetColumn(InspectorPanel, 2);
        Grid.SetRow(InspectorPanel, 0);
    }
}
