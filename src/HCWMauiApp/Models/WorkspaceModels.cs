using WorkspaceManager.Services;

namespace WorkspaceManager;

public class AgentViewModel
{
    public string DirectoryName { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Version { get; set; } = "0.0.0";
    public string Description { get; set; } = "No description provided.";
    public string Category { get; set; } = "Workspace Pack";
    public string IconKey { get; set; } = "pack";
    public string UpdateState { get; set; } = "Unknown source";
    public List<string> ProviderIds { get; set; } = [];
    public List<WorkspaceLink> OfficialLinks { get; set; } = [];
    public List<WorkspaceLink> BestPracticeLinks { get; set; } = [];
    public List<ManifestToolRequirement> RequiredTools { get; set; } = [];
    public List<ManifestMcpServerRequirement> RequiredMcpServers { get; set; } = [];
    public List<string> RequiredFiles { get; set; } = [];
    public string SourceRepository { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = "main";
    public string SourcePath { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public sealed class WorkspaceLink
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class ProviderInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = "Service Providers";
    public string IconAsset { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#38BDF8";
    public string OfficialUrl { get; set; } = string.Empty;
}

public sealed class ProviderGroup
{
    public string Name { get; set; } = string.Empty;
    public List<ProviderInfo> Providers { get; set; } = [];
}

public sealed class WorkspaceFolderNode
{
    public string Label { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Kind { get; set; } = "Folder";
    public bool Exists { get; set; }
    public string Status => Exists ? "Ready" : "Missing";
}

public sealed class ActivityLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Severity { get; set; } = "Info";
    public string Category { get; set; } = "Workspace";
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string DisplayTime => Timestamp.ToString("h:mm:ss tt");
    public string SeverityGlyph => Severity switch
    {
        "Success" => "[OK]",
        "Warning" => "[!!]",
        "Error" => "[XX]",
        _ => "[..]"
    };
    public string AccentColor => Severity switch
    {
        "Success" => "#107C10",
        "Warning" => "#D83B01",
        "Error" => "#A4262C",
        _ => "#0078D4"
    };
}

public sealed class HelperMcpHealth
{
    public bool ScriptExists { get; set; }
    public bool PythonAvailable { get; set; }
    public bool PackageCheckPassed { get; set; }
    public bool IsLinked { get; set; }
    public string ScriptPath { get; set; } = string.Empty;
    public string Status => ScriptExists && PythonAvailable && PackageCheckPassed
        ? "Helper ready"
        : "Helper needs attention";
    public string Summary => $"read_compressed_file + compress_context + optimization_stats | {(IsLinked ? "linked" : "not linked")}";
}

public sealed class WorkspaceActivationRequest
{
    public string RepoRootPath { get; set; } = string.Empty;
    public IReadOnlyList<AgentViewModel> SelectedAgents { get; set; } = [];
    public bool IsDryRun { get; set; }
    public bool IncludeHelperMcp { get; set; }
}

public sealed class WorkspaceActivationResult
{
    public WorkspaceActivationResult(bool succeeded, IReadOnlyList<string> logs)
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
