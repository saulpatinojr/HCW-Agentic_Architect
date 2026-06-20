using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO.Compression;
using System.Diagnostics;
using AgenticWorkspaceManager.Services;

namespace AgenticWorkspaceManager;

public partial class MainPage : ContentPage
{
    private readonly ToolInstallService _toolInstallService;
    private string _repoRootPath;
    public ObservableCollection<AgentViewModel> DiscoveredAgents { get; set; } = new();

    public MainPage(ToolInstallService toolInstallService)
    {
        _toolInstallService = toolInstallService;
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
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo dir = new DirectoryInfo(baseDir);

        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "workspace-config")))
        {
            dir = dir.Parent;
        }

        _repoRootPath = dir?.FullName ?? AppDomain.CurrentDomain.BaseDirectory;
        Log($"Active Root Bound: {Path.GetFileName(_repoRootPath)}");
    }

    private void OnScanWorkspaceClicked(object sender, EventArgs e)
    {
        string agentsPath = Path.Combine(_repoRootPath, "workspace-config", "agents");
        if (!Directory.Exists(agentsPath)) Directory.CreateDirectory(agentsPath);

        DiscoveredAgents.Clear();
        var directories = Directory.GetDirectories(agentsPath);

        foreach (var dir in directories)
        {
            string dirName = Path.GetFileName(dir);
            string friendlyName = dirName;
            string agentsMdPath = Path.Combine(dir, "AGENTS.md");

            if (File.Exists(agentsMdPath))
            {
                var firstLine = File.ReadLines(agentsMdPath).FirstOrDefault(l => l.StartsWith("# "));
                if (!string.IsNullOrEmpty(firstLine))
                {
                    friendlyName = firstLine.Replace("# Persona:", "").Replace("#", "").Trim();
                }
            }

            DiscoveredAgents.Add(new AgentViewModel 
            { 
                DirectoryName = dirName, 
                FriendlyName = friendlyName, 
                FullPath = dir,
                IsSelected = false 
            });
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
                string packName = Path.GetFileNameWithoutExtension(result.FileName);
                string targetDir = Path.Combine(_repoRootPath, "workspace-config", "agents", packName);

                if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                
                ZipFile.ExtractToDirectory(result.FullPath, targetDir);
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
            Log("[*] Preflight: checking/installing required CLI tools...");
            var installLogs = await _toolInstallService.InstallMissingToolsAsync();
            foreach (var line in installLogs) { Log(line); }

            var mcpConfig = new WorkspaceMcpConfig { ActivePersonas = selected.Select(s => s.FriendlyName).ToList() };
            
            if (TokenomicsSwitch.IsToggled)
            {
                string mcpServerPath = Path.Combine(_repoRootPath, "workspace-config", "mcp-servers", "token-compressor", "server.py");
                mcpConfig.McpServers.Add("token-compressor", new { command = "python", args = new[] { mcpServerPath } });
                Log("[+] Agentic Tokenomics Enabled: Token Squeezer MCP linked.");
            }

            string jsonString = JsonSerializer.Serialize(mcpConfig, new JsonSerializerOptions { WriteIndented = true });
            string vscodeDir = Path.Combine(_repoRootPath, ".vscode");
            if (!Directory.Exists(vscodeDir)) Directory.CreateDirectory(vscodeDir);
            
            await File.WriteAllTextAsync(Path.Combine(vscodeDir, "mcp.json"), jsonString);
            
            foreach (var agent in selected) { BindToolchain(agent.FullPath); }
            Log("[✓] Team synchronized to workspace.");
        }
        catch (Exception ex) { Log($"[-] Orchestration fault: {ex.Message}"); }
    }

    private void BindToolchain(string agentPackPath)
    {
        string sourceClaude = Path.Combine(agentPackPath, "CLAUDE.md");
        if (File.Exists(sourceClaude)) { File.Copy(sourceClaude, Path.Combine(_repoRootPath, "CLAUDE.md"), true); }

        string sourceCopilot = Path.Combine(agentPackPath, ".github", "copilot-instructions.md");
        if (File.Exists(sourceCopilot))
        {
            string targetDir = Path.Combine(_repoRootPath, ".github");
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            File.Copy(sourceCopilot, Path.Combine(targetDir, "copilot-instructions.md"), true);
        }
    }

    private async void OnSystemCheckClicked(object sender, EventArgs e)
    {
        Log("=============================");
        Log("INITIATING SYSTEM CHECK...");
        var statusLines = _toolInstallService.GetToolStatusLabels();
        foreach (var status in statusLines) { Log($" {status}"); }

        var missing = _toolInstallService.GetMissingTools().Count;
        Log("=============================");
        Log(missing == 0
            ? "[SYSTEM READY] All required tools are available."
            : $"[WARNING] {missing} tools are missing. Use activation to auto-install via winget where supported.");
    }
}

public class AgentViewModel { public string DirectoryName { get; set; } public string FriendlyName { get; set; } public string FullPath { get; set; } public bool IsSelected { get; set; } }
public class WorkspaceMcpConfig { public Dictionary<string, object> McpServers { get; set; } = new(); public List<string> ActivePersonas { get; set; } = new(); }