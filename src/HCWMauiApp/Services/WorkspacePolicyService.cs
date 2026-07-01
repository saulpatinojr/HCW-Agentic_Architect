using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class WorkspacePolicyService
{
    private const string PolicyFileName = "policy-manifest.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _settingsDir;

    private static readonly IReadOnlyDictionary<string, SecurityProfileDefinition> SecurityProfiles =
        new Dictionary<string, SecurityProfileDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["secure-default"] = new(
                "secure-default",
                "Secure Default",
                "Balanced security posture with pre-commit hooks and workspace guardrails.",
                ["workspace-config/hooks/pre-commit.sh", "workspace-config/instructions/orchestration-guardrails.md"],
                ["Use explicit approval before Apply", "Keep zero-trust hooks enabled"]),
            ["fast-iteration"] = new(
                "fast-iteration",
                "Fast Iteration",
                "Lightest preset for local experimentation while keeping hook validation on.",
                ["workspace-config/hooks/pre-commit.sh"],
                ["Keep preview mode default enabled"]),
            ["enterprise-guarded"] = new(
                "enterprise-guarded",
                "Enterprise Guarded",
                "Strictest preset with hooks plus repository guardrails.",
                ["workspace-config/hooks/pre-commit.sh", "workspace-config/instructions/orchestration-guardrails.md", "workspace-config/manifest.md"],
                ["Require explicit rationale for bypass", "Keep strict workflow mode enabled"])
        };

    private static readonly IReadOnlyDictionary<string, WorkflowBundleDefinition> WorkflowBundles =
        new Dictionary<string, WorkflowBundleDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["feature-development"] = new(
                "feature-development",
                "Feature Development",
                ["brainstorm", "plan", "tdd", "review", "finish"]),
            ["bug-fix"] = new(
                "bug-fix",
                "Bug Fix",
                ["reproduce", "isolate", "fix", "test", "review"]),
            ["security-audit"] = new(
                "security-audit",
                "Security Audit",
                ["inventory", "review", "validate", "report", "finish"])
        };

    private static readonly IReadOnlyDictionary<string, DelegationModeDefinition> DelegationModes =
        new Dictionary<string, DelegationModeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["advisory"] = new("advisory", "Advisory", "Generates guidance only; no tool execution.", false),
            ["implementation"] = new("implementation", "Implementation", "Permits tool-backed changes after checks pass.", true)
        };

    public WorkspacePolicyService(string? settingsDirectory = null)
    {
        _settingsDir = settingsDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIArchitectAgents");
        Directory.CreateDirectory(_settingsDir);
    }

    public WorkspacePolicyManifest Load()
    {
        string path = ResolvePolicyPath();
        if (!File.Exists(path))
        {
            return WorkspacePolicyManifest.Default();
        }

        try
        {
            string json = File.ReadAllText(path);
            var manifest = JsonSerializer.Deserialize<WorkspacePolicyManifest>(json, JsonOptions);
            return manifest ?? WorkspacePolicyManifest.Default();
        }
        catch
        {
            return WorkspacePolicyManifest.Default();
        }
    }

    public async Task SaveAsync(WorkspacePolicyManifest manifest)
    {
        string path = ResolvePolicyPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string json = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public WorkspacePolicyEvaluation Evaluate(
        string repoRootPath,
        IReadOnlyList<PackValidation> packResults,
        WorkspacePolicyManifest? overrideManifest = null)
    {
        var manifest = overrideManifest ?? Load();
        var evaluation = new WorkspacePolicyEvaluation
        {
            SecurityProfile = GetSecurityProfile(manifest.SecurityProfile),
            WorkflowBundle = GetWorkflowBundle(manifest.WorkflowBundle),
            DelegationMode = GetDelegationMode(manifest.DelegationMode),
            Manifest = manifest
        };

        EvaluateSecurityProfile(repoRootPath, evaluation, manifest);
        EvaluateWorkflowBundle(packResults, evaluation, manifest);
        EvaluateDelegationProfile(evaluation, manifest);
        EvaluateQualityThresholds(packResults, evaluation, manifest);

        evaluation.IsAllowed = !evaluation.Errors.Any();
        return evaluation;
    }

    public IReadOnlyList<SecurityProfileDefinition> GetSecurityProfiles() => SecurityProfiles.Values.OrderBy(p => p.DisplayName).ToList();
    public IReadOnlyList<WorkflowBundleDefinition> GetWorkflowBundles() => WorkflowBundles.Values.OrderBy(p => p.DisplayName).ToList();
    public IReadOnlyList<DelegationModeDefinition> GetDelegationModes() => DelegationModes.Values.OrderBy(p => p.DisplayName).ToList();

    public string GetDefaultDelegationProfilesJson()
    {
        var profiles = GetDefaultDelegationProfiles();
        return JsonSerializer.Serialize(profiles, JsonOptions);
    }

    private static SecurityProfileDefinition GetSecurityProfile(string name)
        => SecurityProfiles.TryGetValue(name, out var profile) ? profile : SecurityProfiles["secure-default"];

    private static WorkflowBundleDefinition GetWorkflowBundle(string name)
        => WorkflowBundles.TryGetValue(name, out var bundle) ? bundle : WorkflowBundles["feature-development"];

    private static DelegationModeDefinition GetDelegationMode(string name)
        => DelegationModes.TryGetValue(name, out var mode) ? mode : DelegationModes["advisory"];

    private void EvaluateSecurityProfile(string repoRootPath, WorkspacePolicyEvaluation evaluation, WorkspacePolicyManifest manifest)
    {
        foreach (var requiredAsset in evaluation.SecurityProfile.RequiredAssets)
        {
            string path = Path.Combine(repoRootPath, requiredAsset.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                evaluation.Errors.Add($"Security profile '{evaluation.SecurityProfile.DisplayName}' requires missing asset: {requiredAsset}");
            }
        }

        foreach (var baseline in evaluation.SecurityProfile.Baselines)
        {
            evaluation.Warnings.Add(baseline);
        }

        if (manifest.StrictWorkflowMode && string.IsNullOrWhiteSpace(manifest.BypassReason))
        {
            evaluation.Warnings.Add("Strict workflow mode is enabled; bypass reason is empty.");
        }
    }

    private void EvaluateWorkflowBundle(IReadOnlyList<PackValidation> packResults, WorkspacePolicyEvaluation evaluation, WorkspacePolicyManifest manifest)
    {
        var stages = evaluation.WorkflowBundle.Stages.Select((stage, index) => new WorkflowCheckpoint
        {
            Stage = stage,
            Order = index + 1,
            IsRequired = true
        }).ToList();

        int validPacks = packResults.Count(result => result.Validation.IsValid);
        int packCount = packResults.Count;
        bool allPacksValid = packCount > 0 && validPacks == packCount;
        bool allPacksHaveManifests = packResults.All(result => result.Validation.HasManifest);
        bool anyPackHasTools = packResults.Any(result => result.Validation.Manifest?.RequiredTools.Count > 0);
        bool anyPackHasGuidance = packResults.Any(result => result.Validation.Manifest?.RequiredFiles.Count > 0);

        foreach (var checkpoint in stages)
        {
            checkpoint.IsComplete = checkpoint.Stage switch
            {
                "brainstorm" => packCount > 0,
                "plan" => allPacksHaveManifests,
                "tdd" => anyPackHasTools,
                "review" => anyPackHasGuidance,
                "inventory" => packCount > 0,
                "validate" => allPacksValid,
                "report" => evaluation.Warnings.Count == 0,
                "finish" => allPacksValid && packCount > 0,
                "reproduce" => packCount > 0,
                "isolate" => anyPackHasTools || anyPackHasGuidance,
                "fix" => allPacksValid,
                _ => false
            };
        }

        evaluation.Checkpoints = stages;

        if (manifest.StrictWorkflowMode && stages.Any(stage => !stage.IsComplete))
        {
            evaluation.Errors.Add($"Workflow bundle '{evaluation.WorkflowBundle.DisplayName}' has incomplete checkpoints.");
        }
    }

    private void EvaluateDelegationProfile(WorkspacePolicyEvaluation evaluation, WorkspacePolicyManifest manifest)
    {
        var profiles = ParseDelegationProfiles(manifest.DelegationProfilesJson);
        var health = new PartnerAdapterHealthService().GetHealth();
        var healthMap = health.ToDictionary(item => item.Partner, StringComparer.OrdinalIgnoreCase);

        foreach (var profile in profiles)
        {
            if (!profile.Enabled)
            {
                continue;
            }

            if (!healthMap.TryGetValue(profile.Provider, out var item))
            {
                evaluation.Warnings.Add($"Delegation provider '{profile.Provider}' is not recognized.");
                continue;
            }

            if (!item.IsHealthy)
            {
                evaluation.Warnings.Add($"Delegation profile '{profile.Provider}' is unavailable: {item.Detail}");
                if (string.Equals(profile.Mode, "implementation", StringComparison.OrdinalIgnoreCase))
                {
                    evaluation.Errors.Add($"Implementation mode requires '{profile.Provider}' to be available.");
                }
            }
        }

        evaluation.DelegationProfiles = profiles;
        evaluation.DelegationSummary = profiles.Count == 0
            ? "No delegation profiles configured."
            : string.Join(" | ", profiles.Select(profile => $"{profile.Provider}:{profile.Role}:{profile.Mode}"));
    }

    private void EvaluateQualityThresholds(IReadOnlyList<PackValidation> packResults, WorkspacePolicyEvaluation evaluation, WorkspacePolicyManifest manifest)
    {
        int packCount = packResults.Count;
        if (packCount == 0)
        {
            evaluation.QualityScore = 0;
            evaluation.PlanScore = 0;
            return;
        }

        int whoComplete = packResults.Count(result => result.Validation.Manifest?.Who.IsComplete == true);
        int howComplete = packResults.Count(result => result.Validation.Manifest?.How.IsComplete == true);
        int trustComplete = packResults.Count(result => result.Validation.Manifest?.Trust.IsComplete == true);
        int talkComplete = packResults.Count(result => result.Validation.Manifest?.Talk.IsComplete == true);

        evaluation.QualityScore = (int)Math.Round(((whoComplete + howComplete + trustComplete + talkComplete) / (double)(4 * packCount)) * 100d, MidpointRounding.AwayFromZero);
        evaluation.PlanScore = (int)Math.Round((packResults.Count(result => result.Validation.Manifest?.RequiredTools.Count > 0 || result.Validation.Manifest?.RequiredFiles.Count > 0 || result.Validation.Manifest?.RequiredMcpServers.Count > 0 || result.Validation.Manifest?.RequiredEnvVars.Count > 0) / (double)packCount) * 100d, MidpointRounding.AwayFromZero);

        if (evaluation.QualityScore < manifest.ResearchScoreMin)
        {
            evaluation.Errors.Add($"Research score {evaluation.QualityScore} is below minimum {manifest.ResearchScoreMin}.");
        }

        if (evaluation.PlanScore < manifest.PlanScoreMin)
        {
            evaluation.Errors.Add($"Plan score {evaluation.PlanScore} is below minimum {manifest.PlanScoreMin}.");
        }

        if (manifest.TestPassRequired && !packResults.All(result => result.Validation.IsValid))
        {
            evaluation.Errors.Add("Test pass required but at least one pack is not valid.");
        }

        evaluation.QualitySummary = $"research={evaluation.QualityScore} plan={evaluation.PlanScore} retries={manifest.MaxAutoRetry}";
    }

    private static List<DelegationProfileDefinition> GetDefaultDelegationProfiles()
    {
        return
        [
            new DelegationProfileDefinition("codex", "architect", "advisory", true),
            new DelegationProfileDefinition("claude", "reviewer", "advisory", true),
            new DelegationProfileDefinition("copilot", "security", "advisory", true),
            new DelegationProfileDefinition("gemini", "planner", "implementation", false)
        ];
    }

    private static List<DelegationProfileDefinition> ParseDelegationProfiles(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return GetDefaultDelegationProfiles();
        }

        try
        {
            var profiles = JsonSerializer.Deserialize<List<DelegationProfileDefinition>>(json, JsonOptions);
            return profiles is { Count: > 0 } ? profiles : GetDefaultDelegationProfiles();
        }
        catch
        {
            return GetDefaultDelegationProfiles();
        }
    }

    private string ResolvePolicyPath()
    {
        return Path.Combine(_settingsDir, PolicyFileName);
    }
}

public sealed class WorkspacePolicyManifest
{
    public string SecurityProfile { get; set; } = "secure-default";
    public string WorkflowBundle { get; set; } = "feature-development";
    public string DelegationMode { get; set; } = "advisory";
    public bool StrictWorkflowMode { get; set; } = false;
    public string BypassReason { get; set; } = string.Empty;
    public int ResearchScoreMin { get; set; } = 70;
    public int PlanScoreMin { get; set; } = 75;
    public bool TestPassRequired { get; set; } = true;
    public int MaxAutoRetry { get; set; } = 2;
    public string DelegationProfilesJson { get; set; } = string.Empty;

    public static WorkspacePolicyManifest Default()
    {
        return new WorkspacePolicyManifest
        {
            DelegationProfilesJson = JsonSerializer.Serialize(new List<DelegationProfileDefinition>
            {
                new("codex", "architect", "advisory", true),
                new("claude", "reviewer", "advisory", true),
                new("copilot", "security", "advisory", true),
                new("gemini", "planner", "implementation", false)
            })
        };
    }
}

public sealed record SecurityProfileDefinition(
    string Name,
    string DisplayName,
    string Description,
    IReadOnlyList<string> RequiredAssets,
    IReadOnlyList<string> Baselines);

public sealed record WorkflowBundleDefinition(
    string Name,
    string DisplayName,
    IReadOnlyList<string> Stages);

public sealed record DelegationModeDefinition(
    string Name,
    string DisplayName,
    string Description,
    bool AllowsImplementation);

public sealed class DelegationProfileDefinition
{
    public DelegationProfileDefinition()
    {
    }

    public DelegationProfileDefinition(string provider, string role, string mode, bool enabled)
    {
        Provider = provider;
        Role = role;
        Mode = mode;
        Enabled = enabled;
    }

    public string Provider { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Mode { get; set; } = "advisory";
    public bool Enabled { get; set; }
}

public sealed class WorkspacePolicyEvaluation
{
    public WorkspacePolicyManifest Manifest { get; set; } = WorkspacePolicyManifest.Default();
    public SecurityProfileDefinition SecurityProfile { get; set; } = new("secure-default", "Secure Default", string.Empty, [], []);
    public WorkflowBundleDefinition WorkflowBundle { get; set; } = new("feature-development", "Feature Development", []);
    public DelegationModeDefinition DelegationMode { get; set; } = new("advisory", "Advisory", string.Empty, false);
    public IReadOnlyList<WorkflowCheckpoint> Checkpoints { get; set; } = [];
    public IReadOnlyList<DelegationProfileDefinition> DelegationProfiles { get; set; } = [];
    public List<string> Warnings { get; } = [];
    public List<string> Errors { get; } = [];
    public bool IsAllowed { get; set; }
    public int QualityScore { get; set; }
    public int PlanScore { get; set; }
    public string QualitySummary { get; set; } = string.Empty;
    public string DelegationSummary { get; set; } = string.Empty;
}

public sealed class WorkflowCheckpoint
{
    public string Stage { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsComplete { get; set; }
    public string Status => IsComplete ? "Pass" : "Hold";
}

