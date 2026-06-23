using System.IO.Compression;
using System.Net;
using System.Net.Http;
using WorkspaceManager;
using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class WorkspacePackUpdateServiceTests : IDisposable
{
    private readonly string _repoRoot;

    public WorkspacePackUpdateServiceTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "hcw-pack-update-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer"));
        Directory.CreateDirectory(Path.Combine(_repoRoot, "workspace-config"));
    }

    [Fact]
    public async Task UpdateAsync_WhenArchiveContainsValidPack_ReplacesLocalPack()
    {
        WriteCatalog("1.1.0");
        WriteLocalPack("1.0.0");
        var client = CreateHttpClient(CreateRepoArchiveZip("1.1.0"));
        var service = new WorkspacePackUpdateService(client, new PackManifestService(), new WorkspacePackCatalogService());

        var result = await service.UpdateAsync(_repoRoot, new AgentViewModel
        {
            DirectoryName = "tf-engineer",
            FriendlyName = "Terraform Engineer",
            Version = "1.0.0",
            SourceRepository = "saulpatinojr/HCW-WorkspaceManager",
            SourceBranch = "main",
            SourcePath = "workspace-config/agents/tf-engineer"
        });

        Assert.True(result.Succeeded);
        Assert.Contains(result.Logs, line => line.Contains("Updated tf-engineer"));

        var manifest = File.ReadAllText(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer", "pack.manifest.json"));
        Assert.Contains("\"version\": \"1.1.0\"", manifest);
    }

    [Fact]
    public async Task UpdateAsync_WhenArchiveFailsValidation_ReturnsFailure()
    {
        WriteCatalog("1.1.0");
        WriteLocalPack("1.0.0");
        var client = CreateHttpClient(CreateRepoArchiveZipWithInvalidManifest());
        var service = new WorkspacePackUpdateService(client, new PackManifestService(), new WorkspacePackCatalogService());

        var result = await service.UpdateAsync(_repoRoot, new AgentViewModel
        {
            DirectoryName = "tf-engineer",
            FriendlyName = "Terraform Engineer",
            Version = "1.0.0",
            SourceRepository = "saulpatinojr/HCW-WorkspaceManager",
            SourceBranch = "main",
            SourcePath = "workspace-config/agents/tf-engineer"
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Logs, line => line.Contains("Invalid JSON in pack.manifest.json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("\"version\": \"1.0.0\"", File.ReadAllText(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer", "pack.manifest.json")));
    }

    [Fact]
    public async Task UpdateAsync_WhenChecksumMatches_UpdatesPack()
    {
        WriteLocalPack("1.0.0");

        string stagedPack = Path.Combine(_repoRoot, "staging", "tf-engineer");
        Directory.CreateDirectory(stagedPack);
        File.WriteAllText(Path.Combine(stagedPack, "pack.manifest.json"), $$"""
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "version": "1.1.0",
  "sourceRepository": "saulpatinojr/HCW-WorkspaceManager",
  "sourceBranch": "main",
  "sourcePath": "workspace-config/agents/tf-engineer"
}
""");

        string checksum = WorkspacePackUpdateService.ComputePackChecksum(stagedPack);
        WriteCatalog("1.1.0", checksum);

        var client = CreateHttpClient(CreateRepoArchiveZip("1.1.0"));
        var service = new WorkspacePackUpdateService(client, new PackManifestService(), new WorkspacePackCatalogService());

        var result = await service.UpdateAsync(_repoRoot, new AgentViewModel
        {
            DirectoryName = "tf-engineer",
            FriendlyName = "Terraform Engineer",
            Version = "1.0.0",
            SourceRepository = "saulpatinojr/HCW-WorkspaceManager",
            SourceBranch = "main",
            SourcePath = "workspace-config/agents/tf-engineer"
        });

        Assert.True(result.Succeeded);
        Assert.Contains(result.Logs, line => line.Contains("Checksum verification passed", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("\"version\": \"1.1.0\"", File.ReadAllText(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer", "pack.manifest.json")));
    }

    [Fact]
    public async Task UpdateAsync_WhenChecksumMismatch_ReturnsFailureAndKeepsLocalPack()
    {
        WriteCatalog("1.1.0", "deadbeef");
        WriteLocalPack("1.0.0");
        var client = CreateHttpClient(CreateRepoArchiveZip("1.1.0"));
        var service = new WorkspacePackUpdateService(client, new PackManifestService(), new WorkspacePackCatalogService());

        var result = await service.UpdateAsync(_repoRoot, new AgentViewModel
        {
            DirectoryName = "tf-engineer",
            FriendlyName = "Terraform Engineer",
            Version = "1.0.0",
            SourceRepository = "saulpatinojr/HCW-WorkspaceManager",
            SourceBranch = "main",
            SourcePath = "workspace-config/agents/tf-engineer"
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Logs, line => line.Contains("Checksum mismatch", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("\"version\": \"1.0.0\"", File.ReadAllText(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer", "pack.manifest.json")));
    }

    private void WriteCatalog(string version, string checksum = "")
    {
        File.WriteAllText(Path.Combine(_repoRoot, "workspace-config", "catalog.json"), $$"""
{
  "packs": [
    {
      "id": "tf-engineer",
      "version": "{{version}}",
            "checksum": "{{checksum}}",
      "sourceRepository": "saulpatinojr/HCW-WorkspaceManager",
      "sourceBranch": "main",
      "sourcePath": "workspace-config/agents/tf-engineer"
    }
  ]
}
""");
    }

    private void WriteLocalPack(string version)
    {
        File.WriteAllText(Path.Combine(_repoRoot, "workspace-config", "agents", "tf-engineer", "pack.manifest.json"), $$"""
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "version": "{{version}}",
  "sourceRepository": "saulpatinojr/HCW-WorkspaceManager",
  "sourceBranch": "main",
  "sourcePath": "workspace-config/agents/tf-engineer"
}
""");
    }

    private static HttpClient CreateHttpClient(byte[] archiveBytes)
    {
        return new HttpClient(new FakeHandler(archiveBytes));
    }

    private static byte[] CreateRepoArchiveZip(string version)
    {
        return CreateRepoArchiveBytes(version, includeManifest: true);
    }

    private static byte[] CreateRepoArchiveZipWithInvalidManifest()
    {
        return CreateRepoArchiveBytes("1.1.0", includeManifest: true, invalidManifest: true);
    }

    private static byte[] CreateRepoArchiveBytes(string version, bool includeManifest, bool invalidManifest = false)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            if (includeManifest)
            {
                var entry = archive.CreateEntry("HCW-WorkspaceManager-main/workspace-config/agents/tf-engineer/pack.manifest.json");
                using var writer = new StreamWriter(entry.Open());
                if (invalidManifest)
                {
                    writer.Write("{ invalid-json }");
                }
                else
                {
                    writer.Write($$"""
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "version": "{{version}}",
  "sourceRepository": "saulpatinojr/HCW-WorkspaceManager",
  "sourceBranch": "main",
  "sourcePath": "workspace-config/agents/tf-engineer"
}
""");
                }
            }
        }

        return stream.ToArray();
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
        {
            Directory.Delete(_repoRoot, true);
        }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly byte[] _archiveBytes;

        public FakeHandler(byte[] archiveBytes)
        {
            _archiveBytes = archiveBytes;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_archiveBytes)
            };
            return Task.FromResult(response);
        }
    }
}
