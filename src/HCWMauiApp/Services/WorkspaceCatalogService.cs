using System.IO.Compression;

namespace AgenticWorkspaceManager.Services;

public sealed class WorkspaceCatalogService
{
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
            string friendlyName = GetFriendlyName(directory, directoryName);

            agentList.Add(new AgentViewModel
            {
                DirectoryName = directoryName,
                FriendlyName = friendlyName,
                FullPath = directory,
                IsSelected = false
            });
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
