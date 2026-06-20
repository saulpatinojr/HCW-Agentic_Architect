namespace AgenticWorkspaceManager.Services;

public sealed class TeamAssemblyService
{
    private const string ConflictPolicy = "fail-fast on merged manifest conflicts; do not overwrite conflicting MCP server names";

    private readonly ManifestRequirementsMergeService _mergeService;
    private readonly ToolInstallService _toolInstallService;
    private readonly PackManifestService _packManifestService;
    private readonly WorkspaceMcpConfigBuilderService _workspaceMcpConfigBuilderService;
    private readonly WorkspaceWriterService _workspaceWriterService;

    public TeamAssemblyService(
        ToolInstallService toolInstallService,
        PackManifestService packManifestService,
        ManifestRequirementsMergeService mergeService,
        WorkspaceMcpConfigBuilderService workspaceMcpConfigBuilderService,
        WorkspaceWriterService workspaceWriterService)
    {
        _toolInstallService = toolInstallService;
        _packManifestService = packManifestService;
        _mergeService = mergeService;
        _workspaceMcpConfigBuilderService = workspaceMcpConfigBuilderService;
        _workspaceWriterService = workspaceWriterService;
    }

    public async Task<TeamAssemblyResult> AssembleTeamAsync(TeamAssemblyRequest request)
    {
        var logs = new List<string>();

        logs.Add($"[*] Conflict policy: {ConflictPolicy}.");
        if (request.IsDryRun)
        {
            logs.Add("[*] Dry-run mode enabled: no files will be written and no tool installs will be executed.");
        }

        logs.Add("[*] Validating selected pack manifests...");
        var manifestResults = request.SelectedAgents
            .Select(agent => new PackValidation(agent, _packManifestService.ValidatePack(agent.FullPath)))
            .ToList();

        foreach (var result in manifestResults)
        {
            foreach (var warning in result.Validation.Warnings)
            {
                logs.Add($"[!] {result.Agent.DirectoryName}: {warning}");
            }
        }

        var invalidResults = manifestResults.Where(r => !r.Validation.IsValid).ToList();
        if (invalidResults.Any())
        {
            foreach (var invalid in invalidResults)
            {
                foreach (var error in invalid.Validation.Errors)
                {
                    logs.Add($"[-] {invalid.Agent.DirectoryName}: {error}");
                }
            }

            logs.Add("[-] Activation halted: one or more selected packs have invalid pack.manifest.json files.");
            return new TeamAssemblyResult(false, logs);
        }

        var validManifestCount = manifestResults.Count(r => r.Validation.HasManifest);
        logs.Add($"[+] Manifest validation complete. Valid manifests loaded: {validManifestCount}/{request.SelectedAgents.Count}.");

        var merged = _mergeService.Merge(manifestResults);
        foreach (var warning in merged.Warnings) { logs.Add($"[!] {warning}"); }
        if (!merged.IsValid)
        {
            foreach (var error in merged.Errors) { logs.Add($"[-] {error}"); }
            logs.Add("[-] Activation halted: merged manifest requirements contain conflicts or missing files.");
            return new TeamAssemblyResult(false, logs);
        }

        logs.Add($"[+] Merged requirements: tools={merged.RequiredTools.Count}, files={merged.RequiredFiles.Count}, mcp={merged.RequiredMcpServers.Count}, env={merged.RequiredEnvVars.Count}.");

        if (!request.IsDryRun)
        {
            logs.Add("[*] Preflight: checking/installing required CLI tools...");
            var installLogs = await _toolInstallService.InstallMissingToolsAsync(merged.RequiredTools.Values);
            logs.AddRange(installLogs);
        }
        else
        {
            var dryToolList = merged.RequiredTools.Values
                .Select(t => string.IsNullOrWhiteSpace(t.DisplayName) ? t.Command : $"{t.DisplayName} ({t.Command})")
                .ToArray();
            logs.Add($"[DRY-RUN] Would preflight/install tools: {(dryToolList.Length == 0 ? "none" : string.Join(", ", dryToolList))}");
        }

        var mcpBuild = _workspaceMcpConfigBuilderService.Build(new WorkspaceMcpConfigBuildRequest
        {
            RepoRootPath = request.RepoRootPath,
            SelectedAgents = request.SelectedAgents,
            MergedRequirements = merged,
            IsTokenomicsEnabled = request.IsTokenomicsEnabled
        });
        logs.AddRange(mcpBuild.Logs);
        if (!mcpBuild.Succeeded || mcpBuild.Config is null)
        {
            return new TeamAssemblyResult(false, logs);
        }

        var writerLogs = await _workspaceWriterService.ApplyAsync(new WorkspaceWriteRequest
        {
            RepoRootPath = request.RepoRootPath,
            SelectedAgents = request.SelectedAgents,
            McpConfig = mcpBuild.Config,
            IsDryRun = request.IsDryRun
        });
        logs.AddRange(writerLogs);

        return new TeamAssemblyResult(true, logs);
    }
}
