using WorkspaceManager;
using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class WorkspacePackCatalogServiceTests : IDisposable
{
    private readonly string _repoRoot;
    private readonly WorkspacePackCatalogService _service = new();

    public WorkspacePackCatalogServiceTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "hcw-pack-catalog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_repoRoot, "workspace-config"));
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", "Current")]
    [InlineData("1.0.0", "1.1.0", "Update available")]
    [InlineData("1.2.0", "1.1.0", "Local newer")]
    public void GetUpdateState_WhenCatalogEntryExists_ComparesVersions(string localVersion, string catalogVersion, string expected)
    {
        WriteCatalog(catalogVersion);
        var agent = new AgentViewModel { DirectoryName = "tf-engineer", Version = localVersion };

        var result = _service.GetUpdateState(_repoRoot, agent);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetUpdateState_WhenCatalogEntryMissing_ReturnsUnknownSource()
    {
        WriteCatalog("1.0.0");
        var agent = new AgentViewModel { DirectoryName = "unknown", Version = "1.0.0" };

        var result = _service.GetUpdateState(_repoRoot, agent);

        Assert.Equal("Unknown source", result);
    }

    private void WriteCatalog(string version)
    {
        File.WriteAllText(Path.Combine(_repoRoot, "workspace-config", "catalog.json"), $$"""
{
  "packs": [
    {
      "id": "tf-engineer",
      "version": "{{version}}",
      "sourcePath": "workspace-config/agents/tf-engineer"
    }
  ]
}
""");
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
        {
            Directory.Delete(_repoRoot, true);
        }
    }
}
