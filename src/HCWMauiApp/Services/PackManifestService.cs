using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkspaceManager.Services;

public sealed class PackManifestService
{
    private const string ManifestFileName = "pack.manifest.json";
    private static readonly Regex EnvVarPattern = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

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

        ValidateManifest(result.Manifest, result.Errors, result.Warnings);
        return result;
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
}

public sealed class PackManifest
{
    public int SchemaVersion { get; set; }
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
