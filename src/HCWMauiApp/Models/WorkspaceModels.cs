using AgenticWorkspaceManager.Services;

namespace AgenticWorkspaceManager;

public class AgentViewModel
{
    public string DirectoryName { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public sealed class TeamAssemblyRequest
{
    public string RepoRootPath { get; set; } = string.Empty;
    public IReadOnlyList<AgentViewModel> SelectedAgents { get; set; } = [];
    public bool IsDryRun { get; set; }
    public bool IsTokenomicsEnabled { get; set; }
}

public sealed class TeamAssemblyResult
{
    public TeamAssemblyResult(bool succeeded, IReadOnlyList<string> logs)
    {
        Succeeded = succeeded;
        Logs = logs;
    }

    public bool Succeeded { get; }
    public IReadOnlyList<string> Logs { get; }
}

public class WorkspaceMcpConfig
{
    public Dictionary<string, object> McpServers { get; set; } = new();
    public List<string> ActivePersonas { get; set; } = new();
}

public sealed record PackValidation(AgentViewModel Agent, PackManifestValidationResult Validation);

public sealed class MergedManifestRequirements
{
    public Dictionary<string, ManifestToolRequirement> RequiredTools { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> RequiredFiles { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ManifestMcpServerRequirement> RequiredMcpServers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> RequiredEnvVars { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Warnings { get; } = [];
    public List<string> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;
}
