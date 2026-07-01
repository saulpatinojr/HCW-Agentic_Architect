namespace WorkspaceManager.Services;

public sealed class WorkspaceActivationService
{
    private const string ConflictPolicy = "fail-fast on merged manifest conflicts; do not overwrite conflicting MCP server names";

    private readonly ManifestRequirementsMergeService _mergeService;
    private readonly ToolInstallService _toolInstallService;
    private readonly PackManifestService _packManifestService;
    private readonly WorkspaceMcpConfigBuilderService _workspaceMcpConfigBuilderService;
    private readonly WorkspaceWriterService _workspaceWriterService;
    private readonly WorkspacePolicyService _workspacePolicyService;

    public WorkspaceActivationService(
        ToolInstallService toolInstallService,
        PackManifestService packManifestService,
        ManifestRequirementsMergeService mergeService,
        WorkspaceMcpConfigBuilderService workspaceMcpConfigBuilderService,
        WorkspaceWriterService workspaceWriterService,
        WorkspacePolicyService workspacePolicyService)
    {
        _toolInstallService = toolInstallService;
        _packManifestService = packManifestService;
        _mergeService = mergeService;
        _workspaceMcpConfigBuilderService = workspaceMcpConfigBuilderService;
        _workspaceWriterService = workspaceWriterService;
        _workspacePolicyService = workspacePolicyService;
    }

    public async Task<WorkspaceActivationResult> ActivateAsync(WorkspaceActivationRequest request)
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
            return new WorkspaceActivationResult(false, logs);
        }

        var validManifestCount = manifestResults.Count(r => r.Validation.HasManifest);
        logs.Add($"[+] Manifest validation complete. Valid manifests loaded: {validManifestCount}/{request.SelectedAgents.Count}.");

        logs.Add("[*] Evaluating workspace policy presets...");
        var policy = _workspacePolicyService.Evaluate(request.RepoRootPath, manifestResults);
        logs.Add($"[+] Policy presets: {policy.SecurityProfile.DisplayName} | {policy.WorkflowBundle.DisplayName} | {policy.DelegationMode.DisplayName}.");
        logs.Add($"[+] Policy quality: {policy.QualitySummary}; delegation: {policy.DelegationSummary}.");

        foreach (var checkpoint in policy.Checkpoints)
        {
            logs.Add($"[{(checkpoint.IsComplete ? "+" : "!")}] Checkpoint {checkpoint.Order}:{checkpoint.Stage} => {checkpoint.Status}");
        }

        foreach (var warning in policy.Warnings)
        {
            logs.Add($"[!] {warning}");
        }

        if (policy.Errors.Any())
        {
            foreach (var error in policy.Errors)
            {
                logs.Add(policy.Manifest.StrictWorkflowMode ? $"[-] {error}" : $"[!] {error}");
            }

            if (policy.Manifest.StrictWorkflowMode)
            {
                logs.Add("[-] Activation halted: strict workflow mode blocked policy violations.");
                return new WorkspaceActivationResult(false, logs);
            }

            if (!string.IsNullOrWhiteSpace(policy.Manifest.BypassReason))
            {
                logs.Add($"[!] Policy bypass noted by {Environment.UserName}: {policy.Manifest.BypassReason}");
            }
        }
        var merged = _mergeService.Merge(manifestResults);
        foreach (var warning in merged.Warnings)
        {
            logs.Add($"[!] {warning}");
        }

        if (!merged.IsValid)
        {
            foreach (var error in merged.Errors)
            {
                logs.Add($"[-] {error}");
            }

            logs.Add("[-] Activation halted: merged manifest requirements contain conflicts or missing files.");
            return new WorkspaceActivationResult(false, logs);
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
            IncludeHelperMcp = request.IncludeHelperMcp
        });
        logs.AddRange(mcpBuild.Logs);
        if (!mcpBuild.Succeeded || mcpBuild.Config is null)
        {
            return new WorkspaceActivationResult(false, logs);
        }

        var writerLogs = await _workspaceWriterService.ApplyAsync(new WorkspaceWriteRequest
        {
            RepoRootPath = request.RepoRootPath,
            SelectedAgents = request.SelectedAgents,
            McpConfig = mcpBuild.Config,
            IsDryRun = request.IsDryRun
        });
        logs.AddRange(writerLogs);

        return new WorkspaceActivationResult(true, logs);
    }
}
