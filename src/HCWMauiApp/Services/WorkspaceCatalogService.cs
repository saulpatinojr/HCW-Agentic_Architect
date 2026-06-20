using System.IO.Compression;

namespace WorkspaceManager.Services;

public sealed class WorkspaceCatalogService
{
    private readonly PackManifestService _packManifestService;
    private readonly WorkspacePackCatalogService _packCatalogService;

    public WorkspaceCatalogService()
        : this(new PackManifestService(), new WorkspacePackCatalogService())
    {
    }

    public WorkspaceCatalogService(PackManifestService packManifestService, WorkspacePackCatalogService packCatalogService)
    {
        _packManifestService = packManifestService;
        _packCatalogService = packCatalogService;
    }

    public string DetermineRepositoryRoot(string baseDirectory)
    {
        DirectoryInfo? directory = new(baseDirectory);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "workspace-config")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? baseDirectory;
    }

    public IReadOnlyList<AgentViewModel> DiscoverAgents(string repoRootPath)
    {
        string agentsPath = Path.Combine(repoRootPath, "workspace-config", "agents");
        if (!Directory.Exists(agentsPath))
        {
            Directory.CreateDirectory(agentsPath);
        }

        var agentList = new List<AgentViewModel>();
        foreach (var directory in Directory.GetDirectories(agentsPath))
        {
            string directoryName = Path.GetFileName(directory);
            var validation = _packManifestService.ValidatePack(directory);
            var manifest = validation.Manifest;

            var agent = new AgentViewModel
            {
                DirectoryName = directoryName,
                FriendlyName = string.IsNullOrWhiteSpace(manifest?.DisplayName) ? GetFriendlyName(directory, directoryName) : manifest.DisplayName,
                FullPath = directory,
                Version = string.IsNullOrWhiteSpace(manifest?.Version) ? "0.0.0" : manifest.Version,
                Description = string.IsNullOrWhiteSpace(manifest?.Description) ? "No description provided." : manifest.Description,
                Category = string.IsNullOrWhiteSpace(manifest?.Category) ? "Workspace Pack" : manifest.Category,
                ProviderIds = manifest?.ProviderIds ?? [],
                IconKey = string.IsNullOrWhiteSpace(manifest?.IconKey) ? "pack" : manifest.IconKey,
                OfficialLinks = manifest?.OfficialLinks ?? [],
                BestPracticeLinks = manifest?.BestPracticeLinks ?? [],
                RequiredTools = manifest?.RequiredTools ?? [],
                RequiredMcpServers = manifest?.RequiredMcpServers ?? [],
                RequiredFiles = manifest?.RequiredFiles ?? [],
                SourceRepository = string.IsNullOrWhiteSpace(manifest?.SourceRepository) ? "saulpatinojr/HCW-WorkspaceManager" : manifest.SourceRepository,
                SourceBranch = string.IsNullOrWhiteSpace(manifest?.SourceBranch) ? "main" : manifest.SourceBranch,
                SourcePath = string.IsNullOrWhiteSpace(manifest?.SourcePath) ? $"workspace-config/agents/{directoryName}" : manifest.SourcePath,
                IsSelected = false
            };
            agent.UpdateState = _packCatalogService.GetUpdateState(repoRootPath, agent);
            agentList.Add(agent);
        }

        return agentList;
    }

    public string ImportAgentPack(string repoRootPath, string zipPath)
    {
        string agentsPath = Path.Combine(repoRootPath, "workspace-config", "agents");
        if (!Directory.Exists(agentsPath))
        {
            Directory.CreateDirectory(agentsPath);
        }

        string packName = Path.GetFileNameWithoutExtension(zipPath);
        string targetDir = Path.Combine(agentsPath, packName);

        if (Directory.Exists(targetDir))
        {
            Directory.Delete(targetDir, true);
        }

        ZipFile.ExtractToDirectory(zipPath, targetDir);
        return packName;
    }

    private static string GetFriendlyName(string directory, string directoryName)
    {
        string agentsMdPath = Path.Combine(directory, "AGENTS.md");
        if (!File.Exists(agentsMdPath))
        {
            return directoryName;
        }

        var firstLine = File.ReadLines(agentsMdPath).FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
        return string.IsNullOrEmpty(firstLine)
            ? directoryName
            : firstLine.Replace("# Persona:", string.Empty, StringComparison.Ordinal)
                .Replace("#", string.Empty, StringComparison.Ordinal)
                .Trim();
    }
}
