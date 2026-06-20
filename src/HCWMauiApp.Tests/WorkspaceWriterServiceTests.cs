using AgenticWorkspaceManager;
using AgenticWorkspaceManager.Services;
using System.Text;
using System.Text.Json;

namespace HCWMauiApp.Tests;

public sealed class WorkspaceWriterServiceTests
{
    [Fact]
    public async Task ApplyAsync_WhenDryRun_DoesNotWriteFilesButLogsPlannedActions()
    {
        var fileSystem = new FakeWorkspaceFileSystem();
        string repoRoot = "c:\\repo";
        string agentRoot = "c:\\repo\\workspace-config\\agents\\pack-a";
        fileSystem.Files[Path.Combine(agentRoot, "CLAUDE.md")] = "claude";
        fileSystem.Files[Path.Combine(agentRoot, ".github", "copilot-instructions.md")] = "copilot";

        var service = new WorkspaceWriterService(fileSystem);
        var request = new WorkspaceWriteRequest
        {
            RepoRootPath = repoRoot,
            IsDryRun = true,
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = agentRoot }
            ],
            McpConfig = new WorkspaceMcpConfig { ActivePersonas = ["Pack A"] }
        };

        var logs = await service.ApplyAsync(request);

        Assert.Contains(logs, line => line.Contains("[DRY-RUN] Would write MCP config to:"));
        Assert.Contains(logs, line => line.Contains("[DRY-RUN] Would sync:") && line.Contains("CLAUDE.md"));
        Assert.Contains(logs, line => line.Contains("[DRY-RUN] Would sync:") && line.Contains("copilot-instructions.md"));
        Assert.DoesNotContain(Path.Combine(repoRoot, ".vscode", "mcp.json"), fileSystem.Files.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Empty(fileSystem.CopyOperations);
    }

    [Fact]
    public async Task ApplyAsync_WhenWriteMode_WritesMcpConfigAndCopiesToolchainFiles()
    {
        var fileSystem = new FakeWorkspaceFileSystem();
        string repoRoot = "c:\\repo";
        string agentRoot = "c:\\repo\\workspace-config\\agents\\pack-a";
        fileSystem.Files[Path.Combine(agentRoot, "CLAUDE.md")] = "claude";
        fileSystem.Files[Path.Combine(agentRoot, ".github", "copilot-instructions.md")] = "copilot";

        var service = new WorkspaceWriterService(fileSystem);
        var request = new WorkspaceWriteRequest
        {
            RepoRootPath = repoRoot,
            IsDryRun = false,
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = agentRoot }
            ],
            McpConfig = new WorkspaceMcpConfig
            {
                ActivePersonas = ["Pack A"],
                McpServers = new Dictionary<string, object> { ["local-cloud-query"] = new { command = "python" } }
            }
        };

        var logs = await service.ApplyAsync(request);

        string mcpPath = Path.Combine(repoRoot, ".vscode", "mcp.json");
        Assert.Contains(mcpPath, fileSystem.Files.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine(repoRoot, "CLAUDE.md"), fileSystem.Files.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine(repoRoot, ".github", "copilot-instructions.md"), fileSystem.Files.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(logs, line => line == "[+] Updated .vscode/mcp.json.");
        Assert.Contains(logs, line => line == "[+] Updated CLAUDE.md from selected pack.");
        Assert.Contains(logs, line => line == "[+] Updated .github/copilot-instructions.md from selected pack.");
        Assert.Contains(logs, line => line == "[✓] Team synchronized to workspace.");
    }

    [Fact]
    public async Task ApplyAsync_WhenFilesAlreadyMatch_LogsNoOpUpdates()
    {
        var fileSystem = new FakeWorkspaceFileSystem();
        string repoRoot = "c:\\repo";
        string agentRoot = "c:\\repo\\workspace-config\\agents\\pack-a";
        string sourceClaude = Path.Combine(agentRoot, "CLAUDE.md");
        string sourceCopilot = Path.Combine(agentRoot, ".github", "copilot-instructions.md");
        string targetClaude = Path.Combine(repoRoot, "CLAUDE.md");
        string targetCopilot = Path.Combine(repoRoot, ".github", "copilot-instructions.md");
        string mcpPath = Path.Combine(repoRoot, ".vscode", "mcp.json");

        fileSystem.Files[sourceClaude] = "same-content";
        fileSystem.Files[targetClaude] = "same-content";
        fileSystem.Files[sourceCopilot] = "same-copilot";
        fileSystem.Files[targetCopilot] = "same-copilot";

        var config = new WorkspaceMcpConfig { ActivePersonas = ["Pack A"] };
        fileSystem.Files[mcpPath] = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        var service = new WorkspaceWriterService(fileSystem);
        var request = new WorkspaceWriteRequest
        {
            RepoRootPath = repoRoot,
            IsDryRun = false,
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = agentRoot }
            ],
            McpConfig = config
        };

        var logs = await service.ApplyAsync(request);

        Assert.Contains(logs, line => line == "[=] .vscode/mcp.json already up-to-date.");
        Assert.Contains(logs, line => line == "[=] CLAUDE.md already up-to-date.");
        Assert.Contains(logs, line => line == "[=] .github/copilot-instructions.md already up-to-date.");
    }

    private sealed class FakeWorkspaceFileSystem : IWorkspaceFileSystem
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<(string Source, string Destination)> CopyOperations { get; } = [];

        public bool FileExists(string path) => Files.ContainsKey(path);

        public bool DirectoryExists(string path)
        {
            return Directories.Contains(path);
        }

        public void CreateDirectory(string path)
        {
            Directories.Add(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return Encoding.UTF8.GetBytes(Files[path]);
        }

        public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
        {
            if (!overwrite && Files.ContainsKey(destinationPath))
            {
                throw new InvalidOperationException("Overwrite disabled.");
            }

            Files[destinationPath] = Files[sourcePath];
            CopyOperations.Add((sourcePath, destinationPath));
        }

        public Task<string> ReadAllTextAsync(string path)
        {
            return Task.FromResult(Files[path]);
        }

        public Task WriteAllTextAsync(string path, string content)
        {
            Files[path] = content;
            return Task.CompletedTask;
        }
    }
}
