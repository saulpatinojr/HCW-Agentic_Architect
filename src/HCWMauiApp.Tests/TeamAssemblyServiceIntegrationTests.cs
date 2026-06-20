using AgenticWorkspaceManager;
using AgenticWorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class TeamAssemblyServiceIntegrationTests : IDisposable
{
    private readonly string _repoRoot;

    public TeamAssemblyServiceIntegrationTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "hcw-team-assembly-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_repoRoot);
    }

    [Fact]
    public async Task AssembleTeamAsync_WhenDryRun_DoesNotWriteWorkspaceFiles()
    {
        string agentPath = CreateAgentPack("pack-a", includeToolchainFiles: true);
        CreateMcpServerScript();

        var service = CreateService(isWindows: true, availableCommands: ["terraform"]);
        var request = new TeamAssemblyRequest
        {
            RepoRootPath = _repoRoot,
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = agentPath }
            ],
            IsDryRun = true,
            IsTokenomicsEnabled = true
        };

        var result = await service.AssembleTeamAsync(request);

        Assert.True(result.Succeeded);
        Assert.Contains(result.Logs, line => line.Contains("[DRY-RUN] Would write MCP config to:"));
        Assert.False(File.Exists(Path.Combine(_repoRoot, ".vscode", "mcp.json")));
        Assert.False(File.Exists(Path.Combine(_repoRoot, "CLAUDE.md")));
    }

    [Fact]
    public async Task AssembleTeamAsync_WhenWriteMode_WritesWorkspaceArtifacts()
    {
        string agentPath = CreateAgentPack("pack-a", includeToolchainFiles: true);
        CreateMcpServerScript();

        var service = CreateService(isWindows: true, availableCommands: ["terraform"]);
        var request = new TeamAssemblyRequest
        {
            RepoRootPath = _repoRoot,
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = agentPath }
            ],
            IsDryRun = false,
            IsTokenomicsEnabled = true
        };

        var result = await service.AssembleTeamAsync(request);

        Assert.True(result.Succeeded);
        Assert.True(File.Exists(Path.Combine(_repoRoot, ".vscode", "mcp.json")));
        Assert.True(File.Exists(Path.Combine(_repoRoot, "CLAUDE.md")));
        Assert.True(File.Exists(Path.Combine(_repoRoot, ".github", "copilot-instructions.md")));
        Assert.Contains(result.Logs, line => line == "[✓] Team synchronized to workspace.");
    }

    private TeamAssemblyService CreateService(bool isWindows, IEnumerable<string> availableCommands)
    {
        var executor = new FakeToolCommandExecutor();
        foreach (var command in availableCommands)
        {
            executor.AvailableCommands.Add(command);
        }

        var toolInstallService = new ToolInstallService(executor, new FakeToolPlatform(isWindows));
        var packManifestService = new PackManifestService();
        var mergeService = new ManifestRequirementsMergeService(new DefaultManifestRequirementsEnvironment());
        var mcpBuilder = new WorkspaceMcpConfigBuilderService();
        var writerService = new WorkspaceWriterService(new DefaultWorkspaceFileSystem());

        return new TeamAssemblyService(toolInstallService, packManifestService, mergeService, mcpBuilder, writerService);
    }

    private string CreateAgentPack(string directoryName, bool includeToolchainFiles)
    {
        string agentPath = Path.Combine(_repoRoot, "workspace-config", "agents", directoryName);
        Directory.CreateDirectory(agentPath);
        File.WriteAllText(Path.Combine(agentPath, "AGENTS.md"), "# Persona: Pack A");
        File.WriteAllText(Path.Combine(agentPath, "pack.manifest.json"), """
{
  "schemaVersion": 1,
  "displayName": "Pack A",
  "requiredTools": [
    {
      "displayName": "Terraform",
      "command": "terraform",
      "wingetId": "Hashicorp.Terraform"
    }
  ],
  "requiredFiles": [
    "AGENTS.md"
  ],
  "requiredMcpServers": [
    {
      "name": "local-cloud-query",
      "command": "python",
      "args": [
        "workspace-config/mcp-servers/local-cloud-query/server.py"
      ]
    }
  ]
}
""");

        if (includeToolchainFiles)
        {
            File.WriteAllText(Path.Combine(agentPath, "CLAUDE.md"), "claude instructions");
            string githubDir = Path.Combine(agentPath, ".github");
            Directory.CreateDirectory(githubDir);
            File.WriteAllText(Path.Combine(githubDir, "copilot-instructions.md"), "copilot instructions");
        }

        return agentPath;
    }

    private void CreateMcpServerScript()
    {
        string scriptPath = Path.Combine(_repoRoot, "workspace-config", "mcp-servers", "local-cloud-query");
        Directory.CreateDirectory(scriptPath);
        File.WriteAllText(Path.Combine(scriptPath, "server.py"), "print('ok')");

        string tokenCompressorPath = Path.Combine(_repoRoot, "workspace-config", "mcp-servers", "token-compressor");
        Directory.CreateDirectory(tokenCompressorPath);
        File.WriteAllText(Path.Combine(tokenCompressorPath, "server.py"), "print('token')");
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
        {
            Directory.Delete(_repoRoot, true);
        }
    }

    private sealed class FakeToolPlatform : IToolPlatform
    {
        public FakeToolPlatform(bool isWindows)
        {
            IsWindowsPlatform = isWindows;
        }

        public bool IsWindowsPlatform { get; }
    }

    private sealed class FakeToolCommandExecutor : IToolCommandExecutor
    {
        public HashSet<string> AvailableCommands { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<bool> IsCommandAvailable(string command, bool isWindows)
        {
            return Task.FromResult(AvailableCommands.Contains(command));
        }

        public Task<bool> IsCommandSubcommandAvailable(string command, string subcommand)
        {
            return Task.FromResult(false);
        }

        public Task<bool> InstallWithWingetAsync(string wingetId)
        {
            return Task.FromResult(true);
        }
    }
}
