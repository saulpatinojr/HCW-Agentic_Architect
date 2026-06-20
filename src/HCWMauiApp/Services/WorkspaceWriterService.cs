using System.Text.Json;

namespace AgenticWorkspaceManager.Services;

public sealed class WorkspaceWriterService
{
    private readonly IWorkspaceFileSystem _fileSystem;

    public WorkspaceWriterService()
        : this(new DefaultWorkspaceFileSystem())
    {
    }

    public WorkspaceWriterService(IWorkspaceFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<IReadOnlyList<string>> ApplyAsync(WorkspaceWriteRequest request)
    {
        var logs = new List<string>();

        string jsonString = JsonSerializer.Serialize(request.McpConfig, new JsonSerializerOptions { WriteIndented = true });
        string vscodeDir = Path.Combine(request.RepoRootPath, ".vscode");
        string mcpJsonPath = Path.Combine(vscodeDir, "mcp.json");

        if (!request.IsDryRun)
        {
            if (!_fileSystem.DirectoryExists(vscodeDir))
            {
                _fileSystem.CreateDirectory(vscodeDir);
            }

            bool mcpChanged = await WriteFileIfChangedAsync(mcpJsonPath, jsonString);
            logs.Add(mcpChanged
                ? "[+] Updated .vscode/mcp.json."
                : "[=] .vscode/mcp.json already up-to-date.");

            foreach (var agent in request.SelectedAgents)
            {
                BindToolchain(request.RepoRootPath, agent.FullPath, logs);
            }

            logs.Add("[✓] Team synchronized to workspace.");
            return logs;
        }

        logs.Add($"[DRY-RUN] Would write MCP config to: {mcpJsonPath}");
        foreach (var agent in request.SelectedAgents)
        {
            SimulateBindToolchain(request.RepoRootPath, agent.FullPath, logs);
        }

        logs.Add("[✓] Dry-run complete. No workspace files were changed.");
        return logs;
    }

    private void BindToolchain(string repoRootPath, string agentPackPath, ICollection<string> logs)
    {
        string sourceClaude = Path.Combine(agentPackPath, "CLAUDE.md");
        if (_fileSystem.FileExists(sourceClaude))
        {
            string targetClaude = Path.Combine(repoRootPath, "CLAUDE.md");
            bool changed = CopyFileIfChanged(sourceClaude, targetClaude);
            logs.Add(changed ? "[+] Updated CLAUDE.md from selected pack." : "[=] CLAUDE.md already up-to-date.");
        }

        string sourceCopilot = Path.Combine(agentPackPath, ".github", "copilot-instructions.md");
        if (_fileSystem.FileExists(sourceCopilot))
        {
            string targetDir = Path.Combine(repoRootPath, ".github");
            if (!_fileSystem.DirectoryExists(targetDir))
            {
                _fileSystem.CreateDirectory(targetDir);
            }

            string targetCopilot = Path.Combine(targetDir, "copilot-instructions.md");
            bool changed = CopyFileIfChanged(sourceCopilot, targetCopilot);
            logs.Add(changed
                ? "[+] Updated .github/copilot-instructions.md from selected pack."
                : "[=] .github/copilot-instructions.md already up-to-date.");
        }
    }

    private void SimulateBindToolchain(string repoRootPath, string agentPackPath, ICollection<string> logs)
    {
        string sourceClaude = Path.Combine(agentPackPath, "CLAUDE.md");
        if (_fileSystem.FileExists(sourceClaude))
        {
            logs.Add($"[DRY-RUN] Would sync: {sourceClaude} -> {Path.Combine(repoRootPath, "CLAUDE.md")}");
        }

        string sourceCopilot = Path.Combine(agentPackPath, ".github", "copilot-instructions.md");
        if (_fileSystem.FileExists(sourceCopilot))
        {
            logs.Add($"[DRY-RUN] Would sync: {sourceCopilot} -> {Path.Combine(repoRootPath, ".github", "copilot-instructions.md")}");
        }
    }

    private bool CopyFileIfChanged(string sourcePath, string targetPath)
    {
        if (_fileSystem.FileExists(targetPath))
        {
            var sourceBytes = _fileSystem.ReadAllBytes(sourcePath);
            var targetBytes = _fileSystem.ReadAllBytes(targetPath);
            if (sourceBytes.AsSpan().SequenceEqual(targetBytes))
            {
                return false;
            }
        }

        _fileSystem.CopyFile(sourcePath, targetPath, true);
        return true;
    }

    private async Task<bool> WriteFileIfChangedAsync(string path, string content)
    {
        if (_fileSystem.FileExists(path))
        {
            var existing = await _fileSystem.ReadAllTextAsync(path);
            if (string.Equals(existing, content, StringComparison.Ordinal))
            {
                return false;
            }
        }

        await _fileSystem.WriteAllTextAsync(path, content);
        return true;
    }
}

public sealed class WorkspaceWriteRequest
{
    public string RepoRootPath { get; set; } = string.Empty;
    public IReadOnlyList<AgentViewModel> SelectedAgents { get; set; } = [];
    public WorkspaceMcpConfig McpConfig { get; set; } = new();
    public bool IsDryRun { get; set; }
}

public interface IWorkspaceFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    byte[] ReadAllBytes(string path);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
}

public sealed class DefaultWorkspaceFileSystem : IWorkspaceFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite) => File.Copy(sourcePath, destinationPath, overwrite);
    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
    public Task WriteAllTextAsync(string path, string content) => File.WriteAllTextAsync(path, content);
}
