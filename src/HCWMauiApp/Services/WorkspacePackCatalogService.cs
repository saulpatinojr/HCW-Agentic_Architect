using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class WorkspacePackCatalogService
{
    private const string CatalogRelativePath = "workspace-config/catalog.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public IReadOnlyList<PackCatalogEntry> LoadCatalog(string repoRootPath)
    {
        string catalogPath = Path.Combine(repoRootPath, CatalogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(catalogPath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(catalogPath);
            return JsonSerializer.Deserialize<PackCatalog>(json, JsonOptions)?.Packs ?? [];
        }
        catch
        {
            return [];
        }
    }

    public string GetUpdateState(string repoRootPath, AgentViewModel agent)
    {
        var catalogEntry = LoadCatalog(repoRootPath)
            .FirstOrDefault(p => p.Id.Equals(agent.DirectoryName, StringComparison.OrdinalIgnoreCase));

        if (catalogEntry is null || string.IsNullOrWhiteSpace(catalogEntry.Version))
        {
            return "Unknown source";
        }

        if (!TryParseVersion(agent.Version, out var localVersion) || !TryParseVersion(catalogEntry.Version, out var remoteVersion))
        {
            return catalogEntry.Version.Equals(agent.Version, StringComparison.OrdinalIgnoreCase)
                ? "Current"
                : "Unknown source";
        }

        int comparison = localVersion.CompareTo(remoteVersion);
        return comparison switch
        {
            < 0 => "Update available",
            > 0 => "Local newer",
            _ => "Current"
        };
    }

    private static bool TryParseVersion(string value, out Version version)
    {
        version = new Version(0, 0, 0);
        return Version.TryParse(value, out version!);
    }
}

public sealed class PackCatalog
{
    public List<PackCatalogEntry> Packs { get; set; } = [];
}

public sealed class PackCatalogEntry
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public string SourceRepository { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = "main";
    public string SourcePath { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
}
