namespace WorkspaceManager.Services;

public sealed class WorkspaceMcpConfigBuilderService
{
    public WorkspaceMcpConfigBuildResult Build(WorkspaceMcpConfigBuildRequest request)
    {
        var logs = new List<string>();
        var mcpConfig = new WorkspaceMcpConfig
        {
            ActivePersonas = request.SelectedAgents.Select(s => s.FriendlyName).ToList()
        };

        foreach (var mcp in request.MergedRequirements.RequiredMcpServers.Values)
        {
            mcpConfig.McpServers[mcp.Name] = new
            {
                command = mcp.Command,
                args = ResolveMcpArgs(request.RepoRootPath, mcp.Args).ToArray()
            };
        }

        if (request.IncludeHelperMcp)
        {
            if (mcpConfig.McpServers.ContainsKey("token-compressor"))
            {
                logs.Add("[-] Activation halted: MCP server name conflict for 'token-compressor'. Conflict policy prohibits overwrite.");
                return new WorkspaceMcpConfigBuildResult(false, null, logs);
            }

            string mcpServerPath = Path.Combine(request.RepoRootPath, "workspace-config", "mcp-servers", "token-compressor", "server.py");
            mcpConfig.McpServers.Add("token-compressor", new { command = "python", args = new[] { mcpServerPath } });
            logs.Add("[+] Helper MCP enabled: token-compressor linked.");
        }

        return new WorkspaceMcpConfigBuildResult(true, mcpConfig, logs);
    }

    private static IEnumerable<string> ResolveMcpArgs(string repoRootPath, IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (Path.IsPathRooted(arg))
            {
                yield return arg;
                continue;
            }

            var normalized = arg.Replace('/', Path.DirectorySeparatorChar);
            yield return Path.Combine(repoRootPath, normalized);
        }
    }
}

public sealed class WorkspaceMcpConfigBuildRequest
{
    public string RepoRootPath { get; set; } = string.Empty;
    public IReadOnlyList<AgentViewModel> SelectedAgents { get; set; } = [];
    public MergedManifestRequirements MergedRequirements { get; set; } = new();
    public bool IncludeHelperMcp { get; set; }
}

public sealed class WorkspaceMcpConfigBuildResult
{
    public WorkspaceMcpConfigBuildResult(bool succeeded, WorkspaceMcpConfig? config, IReadOnlyList<string> logs)
    {
        Succeeded = succeeded;
        Config = config;
        Logs = logs;
    }

    public bool Succeeded { get; }
    public WorkspaceMcpConfig? Config { get; }
    public IReadOnlyList<string> Logs { get; }
}
