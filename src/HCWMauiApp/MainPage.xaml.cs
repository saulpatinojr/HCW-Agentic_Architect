using System.Collections.ObjectModel;
using AgenticWorkspaceManager.Services;

namespace AgenticWorkspaceManager;

public partial class MainPage : ContentPage
{
    private readonly WorkspaceCatalogService _workspaceCatalogService;
    private readonly WorkspaceSystemCheckService _workspaceSystemCheckService;
    private readonly TeamAssemblyService _teamAssemblyService;
    private string _repoRootPath;
    public ObservableCollection<AgentViewModel> DiscoveredAgents { get; set; } = new();

    public MainPage(
        WorkspaceCatalogService workspaceCatalogService,
        WorkspaceSystemCheckService workspaceSystemCheckService,
        TeamAssemblyService teamAssemblyService)
    {
        _workspaceCatalogService = workspaceCatalogService;
        _workspaceSystemCheckService = workspaceSystemCheckService;
        _teamAssemblyService = teamAssemblyService;
        InitializeComponent();
        AgentsCollectionView.ItemsSource = DiscoveredAgents;
        DetermineRepositoryRoot();
    }

    private void Log(string message)
    {
        ConsoleOutput.Text += $"> {message}\n";
    }

    private void DetermineRepositoryRoot()
    {
        _repoRootPath = _workspaceCatalogService.DetermineRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
        Log($"Active Root Bound: {Path.GetFileName(_repoRootPath)}");
    }

    private void OnScanWorkspaceClicked(object sender, EventArgs e)
    {
        DiscoveredAgents.Clear();
        foreach (var agent in _workspaceCatalogService.DiscoverAgents(_repoRootPath))
        {
            DiscoveredAgents.Add(agent);
        }
        Log($"[✓] Discovered {DiscoveredAgents.Count} agent packs.");
    }

    private async void OnImportPackClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an Agent Pack (.zip)"
            });

            if (result != null && result.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string packName = _workspaceCatalogService.ImportAgentPack(_repoRootPath, result.FullPath);
                Log($"[+] Ingested Agent Pack: {packName}");
                OnScanWorkspaceClicked(null, null);
            }
        }
        catch (Exception ex) { Log($"[-] Ingestion failed: {ex.Message}"); }
    }

    private async void OnAssembleTeamClicked(object sender, EventArgs e)
    {
        var selected = DiscoveredAgents.Where(a => a.IsSelected).ToList();
        if (!selected.Any()) { Log("[-] Selection Missing: Select an agent first."); return; }

        try
        {
            var result = await _teamAssemblyService.AssembleTeamAsync(new TeamAssemblyRequest
            {
                RepoRootPath = _repoRootPath,
                SelectedAgents = selected,
                IsDryRun = DryRunSwitch.IsToggled,
                IsTokenomicsEnabled = TokenomicsSwitch.IsToggled
            });

            foreach (var line in result.Logs)
            {
                Log(line);
            }
        }
        catch (Exception ex) { Log($"[-] Orchestration fault: {ex.Message}"); }
    }

    private async void OnSystemCheckClicked(object sender, EventArgs e)
    {
        var lines = await _workspaceSystemCheckService.RunAsync(_repoRootPath, DiscoveredAgents);
        foreach (var line in lines)
        {
            Log(line);
        }
    }
}
