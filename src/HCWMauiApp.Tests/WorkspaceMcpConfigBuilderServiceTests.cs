using WorkspaceManager;
using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class WorkspaceMcpConfigBuilderServiceTests
{
    [Fact]
    public void Build_WhenHelperMcpDisabled_MapsMergedServersAndActivePersonas()
    {
        var service = new WorkspaceMcpConfigBuilderService();
        var merged = new MergedManifestRequirements();
        merged.RequiredMcpServers["local-cloud-query"] = new ManifestMcpServerRequirement
        {
            Name = "local-cloud-query",
            Command = "python",
            Args = ["workspace-config/mcp-servers/local-cloud-query/server.py"]
        };

        var result = service.Build(new WorkspaceMcpConfigBuildRequest
        {
            RepoRootPath = "c:\\repo",
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = "c:\\repo\\workspace-config\\agents\\pack-a" }
            ],
            MergedRequirements = merged,
            IncludeHelperMcp = false
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Config);
        Assert.Single(result.Config!.ActivePersonas);
        Assert.Contains("Pack A", result.Config.ActivePersonas);
        Assert.True(result.Config.McpServers.ContainsKey("local-cloud-query"));
        Assert.Empty(result.Logs);
    }

    [Fact]
    public void Build_WhenHelperMcpEnabled_AddsTokenCompressorServer()
    {
        var service = new WorkspaceMcpConfigBuilderService();

        var result = service.Build(new WorkspaceMcpConfigBuildRequest
        {
            RepoRootPath = "c:\\repo",
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = "c:\\repo\\workspace-config\\agents\\pack-a" }
            ],
            MergedRequirements = new MergedManifestRequirements(),
            IncludeHelperMcp = true
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Config);
        Assert.True(result.Config!.McpServers.ContainsKey("token-compressor"));
        Assert.Contains(result.Logs, line => line == "[+] Helper MCP enabled: token-compressor linked.");
    }

    [Fact]
    public void Build_WhenHelperMcpServerNameAlreadyExists_ReturnsConflictFailure()
    {
        var service = new WorkspaceMcpConfigBuilderService();
        var merged = new MergedManifestRequirements();
        merged.RequiredMcpServers["token-compressor"] = new ManifestMcpServerRequirement
        {
            Name = "token-compressor",
            Command = "python",
            Args = ["custom-server.py"]
        };

        var result = service.Build(new WorkspaceMcpConfigBuildRequest
        {
            RepoRootPath = "c:\\repo",
            SelectedAgents =
            [
                new AgentViewModel { DirectoryName = "pack-a", FriendlyName = "Pack A", FullPath = "c:\\repo\\workspace-config\\agents\\pack-a" }
            ],
            MergedRequirements = merged,
            IncludeHelperMcp = true
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Config);
        Assert.Contains(result.Logs, line => line == "[-] Activation halted: MCP server name conflict for 'token-compressor'. Conflict policy prohibits overwrite.");
    }
}
