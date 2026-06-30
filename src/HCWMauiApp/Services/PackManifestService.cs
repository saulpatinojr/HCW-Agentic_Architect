using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkspaceManager.Services;

public sealed class PackManifestService
{
    private const string ManifestFileName = "pack.manifest.json";
    private static readonly Regex EnvVarPattern = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    private static readonly string[] DefaultTrustPolicies = ["workspace-default-identity-policy"];
    private static readonly string[] DefaultTrustGuardrails = ["inherit repository guardrails"];
    private static readonly string[] DefaultTalkChannels = ["chat"];
    private static readonly string[] DefaultTalkArtifacts = ["AGENTS.md"];
    private const string DefaultTalkStyle = "structured";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public PackManifestValidationResult ValidatePack(string packPath)
    {
        var result = new PackManifestValidationResult
        {
            PackPath = packPath,
            ManifestPath = Path.Combine(packPath, ManifestFileName)
        };

        if (!Directory.Exists(packPath))
        {
            result.Errors.Add("Pack directory does not exist.");
            return result;
        }

        if (!File.Exists(result.ManifestPath))
        {
            result.Warnings.Add("No pack.manifest.json found. Falling back to legacy pack behavior.");
            return result;
        }

        result.HasManifest = true;

        try
        {
            var json = File.ReadAllText(result.ManifestPath);
            result.Manifest = JsonSerializer.Deserialize<PackManifest>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Invalid JSON in pack.manifest.json: {ex.Message}");
            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unable to read pack.manifest.json: {ex.Message}");
            return result;
        }

        if (result.Manifest is null)
        {
            result.Errors.Add("pack.manifest.json is empty or could not be parsed.");
            return result;
        }

        NormalizeManifest(result.Manifest, packPath);
        ValidateManifest(result.Manifest, result.Errors, result.Warnings);
        return result;
    }

    private static void NormalizeManifest(PackManifest manifest, string packPath)
    {
        manifest.Who ??= new ManifestWhoDimension();
        manifest.How ??= new ManifestHowDimension();
        manifest.Trust ??= new ManifestTrustDimension();
        manifest.Talk ??= new ManifestTalkDimension();

        string fallbackRole = FirstNonEmpty(
            manifest.Who.Role,
            manifest.DisplayName,
            ReadFirstHeading(packPath),
            manifest.Id);
        manifest.Who.Role = fallbackRole;
        manifest.Who.Persona = FirstNonEmpty(manifest.Who.Persona, manifest.Category, "Workspace Pack");
        manifest.Who.Summary = FirstNonEmpty(manifest.Who.Summary, manifest.Description);

        if (manifest.Who.Responsibilities.Count == 0 && manifest.ProviderIds.Count > 0)
        {
            manifest.Who.Responsibilities = manifest.ProviderIds
                .Select(providerId => $"Owns {providerId} provider workflows.")
                .ToList();
        }

        if (manifest.How.Capabilities.Count == 0)
        {
            manifest.How.Capabilities = manifest.Tools.Count > 0
                ? [.. manifest.Tools]
                : manifest.RequiredTools
                    .Select(tool => string.IsNullOrWhiteSpace(tool.Command) ? tool.DisplayName : tool.Command)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
        }

        if (manifest.How.ExecutionModes.Count == 0)
        {
            bool canEdit = manifest.Tools.Any(tool => tool.Contains("edit", StringComparison.OrdinalIgnoreCase));
            manifest.How.ExecutionModes = [canEdit ? "implementation" : "advisory"];
        }

        if (manifest.How.Handoffs.Count == 0 && manifest.Handoffs.Count > 0)
        {
            manifest.How.Handoffs = manifest.Handoffs
                .Select(handoff => new ManifestHandoff
                {
                    Label = handoff.Label,
                    Agent = handoff.Agent
                })
                .ToList();
        }

        if (manifest.Trust.IdentityPolicyRefs.Count == 0)
        {
            manifest.Trust.IdentityPolicyRefs = [.. DefaultTrustPolicies];
        }

        if (manifest.Trust.Guardrails.Count == 0)
        {
            manifest.Trust.Guardrails = manifest.RequiredEnvVars.Count > 0 || manifest.RequiredFiles.Count > 0
                ? manifest.RequiredEnvVars
                    .Select(envVar => $"Require environment variable: {envVar}")
                    .Concat(manifest.RequiredFiles.Select(requiredFile => $"Require workspace file: {requiredFile}"))
                    .ToList()
                : [.. DefaultTrustGuardrails];
        }

        if (manifest.Trust.ValidationRules.Count == 0)
        {
            manifest.Trust.ValidationRules =
            [
                "schemaVersion must be greater than 0",
                "all required dimensions must contain metadata"
            ];
        }

        if (manifest.Talk.Channels.Count == 0)
        {
            manifest.Talk.Channels = [.. DefaultTalkChannels];
        }

        if (manifest.Talk.Artifacts.Count == 0)
        {
            var artifacts = new List<string>();
            if (File.Exists(Path.Combine(packPath, "AGENTS.md")))
            {
                artifacts.Add("AGENTS.md");
            }

            if (File.Exists(Path.Combine(packPath, "CLAUDE.md")))
            {
                artifacts.Add("CLAUDE.md");
            }

            manifest.Talk.Artifacts = artifacts.Count > 0 ? artifacts : [.. DefaultTalkArtifacts];
        }

        manifest.Talk.ResponseStyle = FirstNonEmpty(manifest.Talk.ResponseStyle, DefaultTalkStyle);
        manifest.DisplayName = FirstNonEmpty(manifest.DisplayName, manifest.Who.Role);
        manifest.Description = FirstNonEmpty(manifest.Description, manifest.Who.Summary);
        manifest.Category = FirstNonEmpty(manifest.Category, manifest.Who.Persona);
    }

    private static void ValidateManifest(PackManifest manifest, ICollection<string> errors, ICollection<string> warnings)
    {
        if (manifest.SchemaVersion <= 0)
        {
            errors.Add("schemaVersion must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(manifest.DisplayName))
        {
            errors.Add("displayName is required.");
        }

        ValidateDimension("who", manifest.Who.IsComplete, errors);
        ValidateDimension("how", manifest.How.IsComplete, errors);
        ValidateDimension("trust", manifest.Trust.IsComplete, errors);
        ValidateDimension("talk", manifest.Talk.IsComplete, errors);

        var toolCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tool in manifest.RequiredTools)
        {
            if (string.IsNullOrWhiteSpace(tool.Command))
            {
                errors.Add("Each requiredTools entry must include a non-empty command.");
                continue;
            }

            if (!toolCommands.Add(tool.Command))
            {
                errors.Add($"Duplicate tool command detected in requiredTools: {tool.Command}");
            }
        }

        var mcpServerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var server in manifest.RequiredMcpServers)
        {
            if (string.IsNullOrWhiteSpace(server.Name))
            {
                errors.Add("Each requiredMcpServers entry must include a non-empty name.");
                continue;
            }

            if (!mcpServerNames.Add(server.Name))
            {
                errors.Add($"Duplicate MCP server name detected: {server.Name}");
            }

            if (string.IsNullOrWhiteSpace(server.Command))
            {
                errors.Add($"requiredMcpServers.{server.Name} must include a non-empty command.");
            }
        }

        foreach (var relativePath in manifest.RequiredFiles)
        {
            if (Path.IsPathRooted(relativePath))
            {
                errors.Add($"requiredFiles must use relative paths only: {relativePath}");
            }
        }

        var envVarNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var envVar in manifest.RequiredEnvVars)
        {
            if (string.IsNullOrWhiteSpace(envVar))
            {
                errors.Add("requiredEnvVars cannot include empty names.");
                continue;
            }

            if (!EnvVarPattern.IsMatch(envVar))
            {
                errors.Add($"Invalid environment variable name in requiredEnvVars: {envVar}");
                continue;
            }

            if (!envVarNames.Add(envVar))
            {
                errors.Add($"Duplicate environment variable in requiredEnvVars: {envVar}");
            }
        }

        if (!manifest.RequiredTools.Any()
            && !manifest.RequiredFiles.Any()
            && !manifest.RequiredMcpServers.Any()
            && !manifest.RequiredEnvVars.Any())
        {
            warnings.Add("Manifest has no requiredTools, requiredFiles, requiredMcpServers, or requiredEnvVars entries.");
        }
    }

    private static void ValidateDimension(string name, bool isComplete, ICollection<string> errors)
    {
        if (!isComplete)
        {
            errors.Add($"{name} dimension must include at least one field.");
        }
    }

    private static string ReadFirstHeading(string packPath)
    {
        string agentsPath = Path.Combine(packPath, "AGENTS.md");
        if (!File.Exists(agentsPath))
        {
            return string.Empty;
        }

        string? heading = File.ReadLines(agentsPath)
            .FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(heading))
        {
            return string.Empty;
        }

        return heading
            .Replace("#", string.Empty, StringComparison.Ordinal)
            .Replace("Instructions", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }
}

public sealed class PackManifest
{
    public int SchemaVersion { get; set; }
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = "0.0.0";
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> ProviderIds { get; set; } = [];
    public string IconKey { get; set; } = string.Empty;
    public List<WorkspaceLink> OfficialLinks { get; set; } = [];
    public List<WorkspaceLink> BestPracticeLinks { get; set; } = [];
    public string SourceRepository { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = "main";
    public string SourcePath { get; set; } = string.Empty;
    public List<ManifestToolRequirement> RequiredTools { get; set; } = [];
    public List<string> RequiredFiles { get; set; } = [];
    public List<ManifestMcpServerRequirement> RequiredMcpServers { get; set; } = [];
    public List<string> RequiredEnvVars { get; set; } = [];
    public ManifestWhoDimension Who { get; set; } = new();
    public ManifestHowDimension How { get; set; } = new();
    public ManifestTrustDimension Trust { get; set; } = new();
    public ManifestTalkDimension Talk { get; set; } = new();
    public List<string> Tools { get; set; } = [];
    public List<ManifestLegacyHandoff> Handoffs { get; set; } = [];
}

public sealed class ManifestWhoDimension
{
    public string Role { get; set; } = string.Empty;
    public string Persona { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Responsibilities { get; set; } = [];
    public bool IsComplete => !string.IsNullOrWhiteSpace(Role)
        || !string.IsNullOrWhiteSpace(Summary)
        || Responsibilities.Count > 0;
}

public sealed class ManifestHowDimension
{
    public List<string> Capabilities { get; set; } = [];
    public List<string> ExecutionModes { get; set; } = [];
    public List<ManifestHandoff> Handoffs { get; set; } = [];
    public bool IsComplete => Capabilities.Count > 0
        || ExecutionModes.Count > 0
        || Handoffs.Count > 0;
}

public sealed class ManifestTrustDimension
{
    public List<string> IdentityPolicyRefs { get; set; } = [];
    public List<string> Guardrails { get; set; } = [];
    public List<string> ValidationRules { get; set; } = [];
    public bool IsComplete => IdentityPolicyRefs.Count > 0
        || Guardrails.Count > 0
        || ValidationRules.Count > 0;
}

public sealed class ManifestTalkDimension
{
    public List<string> Channels { get; set; } = [];
    public List<string> Artifacts { get; set; } = [];
    public string ResponseStyle { get; set; } = string.Empty;
    public bool IsComplete => Channels.Count > 0
        || Artifacts.Count > 0
        || !string.IsNullOrWhiteSpace(ResponseStyle);
}

public sealed class ManifestHandoff
{
    public string Label { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
}

public sealed class ManifestLegacyHandoff
{
    public string Label { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
}

public sealed class ManifestToolRequirement
{
    public string DisplayName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string WingetId { get; set; } = string.Empty;
}

public sealed class ManifestMcpServerRequirement
{
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = [];
}

public sealed class PackManifestValidationResult
{
    public string PackPath { get; set; } = string.Empty;
    public string ManifestPath { get; set; } = string.Empty;
    public bool HasManifest { get; set; }
    public PackManifest? Manifest { get; set; }
    public List<string> Warnings { get; } = [];
    public List<string> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;
}


