using AgenticWorkspaceManager;
using AgenticWorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class WorkspaceMcpConfigBuilderServiceTests
{
    [Fact]
    public void Build_WhenTokenomicsDisabled_MapsMergedServersAndActivePersonas()
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
            IsTokenomicsEnabled = false
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Config);
        Assert.Single(result.Config!.ActivePersonas);
        Assert.Contains("Pack A", result.Config.ActivePersonas);
        Assert.True(result.Config.McpServers.ContainsKey("local-cloud-query"));
        Assert.Empty(result.Logs);
    }

    [Fact]
    public void Build_WhenTokenomicsEnabled_AddsTokenCompressorServer()
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
            IsTokenomicsEnabled = true
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Config);
        Assert.True(result.Config!.McpServers.ContainsKey("token-compressor"));
        Assert.Contains(result.Logs, line => line == "[+] Agentic Tokenomics Enabled: Token Squeezer MCP linked.");
    }

    [Fact]
    public void Build_WhenTokenomicsServerNameAlreadyExists_ReturnsConflictFailure()
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
            IsTokenomicsEnabled = true
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Config);
        Assert.Contains(result.Logs, line => line == "[-] Activation halted: MCP server name conflict for 'token-compressor'. Conflict policy prohibits overwrite.");
    }
}
