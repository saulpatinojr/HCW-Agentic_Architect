using WorkspaceManager.Services;
using System.IO.Compression;

namespace HCWMauiApp.Tests;

public sealed class WorkspaceCatalogServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly WorkspaceCatalogService _service = new();

    public WorkspaceCatalogServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "hcw-catalog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void DetermineRepositoryRoot_WhenWorkspaceConfigExistsInParent_FindsRepoRoot()
    {
        string repoRoot = Path.Combine(_tempRoot, "repo");
        string nested = Path.Combine(repoRoot, "src", "bin", "Debug");
        Directory.CreateDirectory(Path.Combine(repoRoot, "workspace-config"));
        Directory.CreateDirectory(nested);

        var result = _service.DetermineRepositoryRoot(nested);

        Assert.Equal(repoRoot, result);
    }

    [Fact]
    public void DiscoverAgents_WhenAgentsExist_ReturnsFriendlyNamesFromAgentsMd()
    {
        string repoRoot = Path.Combine(_tempRoot, "repo-discover");
        string agentPath = Path.Combine(repoRoot, "workspace-config", "agents", "pack-a");
        Directory.CreateDirectory(agentPath);
        File.WriteAllText(Path.Combine(agentPath, "AGENTS.md"), "# Persona: Platform Architect");

        var agents = _service.DiscoverAgents(repoRoot);

        var agent = Assert.Single(agents);
        Assert.Equal("pack-a", agent.DirectoryName);
        Assert.Equal("Platform Architect", agent.FriendlyName);
    }

    [Fact]
    public void ImportAgentPack_WhenZipProvided_ExtractsIntoAgentsFolder()
    {
        string repoRoot = Path.Combine(_tempRoot, "repo-import");
        Directory.CreateDirectory(repoRoot);

        string sourceFolder = Path.Combine(_tempRoot, "pack-a-source");
        Directory.CreateDirectory(sourceFolder);
        File.WriteAllText(Path.Combine(sourceFolder, "AGENTS.md"), "# Persona: Pack A");

        string zipPath = Path.Combine(_tempRoot, "pack-a.zip");
        ZipFile.CreateFromDirectory(sourceFolder, zipPath);

        string packName = _service.ImportAgentPack(repoRoot, zipPath);
        string extractedAgents = Path.Combine(repoRoot, "workspace-config", "agents", "pack-a", "AGENTS.md");

        Assert.Equal("pack-a", packName);
        Assert.True(File.Exists(extractedAgents));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }
}
