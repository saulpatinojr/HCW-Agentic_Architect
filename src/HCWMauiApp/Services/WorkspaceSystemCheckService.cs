namespace WorkspaceManager.Services;

public sealed class WorkspaceSystemCheckService
{
    private readonly ToolInstallService _toolInstallService;
    private readonly PackManifestService _packManifestService;

    public WorkspaceSystemCheckService(ToolInstallService toolInstallService, PackManifestService packManifestService)
    {
        _toolInstallService = toolInstallService;
        _packManifestService = packManifestService;
    }

    public async Task<IReadOnlyList<string>> RunAsync(string repoRootPath, IEnumerable<AgentViewModel> discoveredAgents)
    {
        var logs = new List<string>
        {
            "=============================",
            "INITIATING SYSTEM CHECK...",
            $"[*] Platform: {DeviceInfo.Platform}"
        };

        logs.AddRange(GetPlatformSpecificNotes());

        var workspaceManifestPath = Path.Combine(repoRootPath, "workspace-config", "manifest.md");
        if (File.Exists(workspaceManifestPath))
        {
            logs.Add($"[✓] Workspace manifest found: {workspaceManifestPath}");
        }
        else
        {
            logs.Add($"[X] Workspace manifest missing: {workspaceManifestPath}");
        }

        var additionalTools = new List<ManifestToolRequirement>();
        var requiredEnvVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int missingRequiredFiles = 0;
        int missingMcpPaths = 0;

        foreach (var agent in discoveredAgents)
        {
            var validation = _packManifestService.ValidatePack(agent.FullPath);
            if (!validation.HasManifest)
            {
                logs.Add($"[!] {agent.DirectoryName}: no pack.manifest.json; using legacy pack behavior.");
                continue;
            }

            foreach (var warning in validation.Warnings)
            {
                logs.Add($"[!] {agent.DirectoryName}: {warning}");
            }

            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    logs.Add($"[X] {agent.DirectoryName}: {error}");
                }

                continue;
            }

            if (validation.Manifest is null)
            {
                continue;
            }

            additionalTools.AddRange(validation.Manifest.RequiredTools);

            foreach (var requiredFile in validation.Manifest.RequiredFiles)
            {
                var normalized = requiredFile.Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(agent.FullPath, normalized);
                if (File.Exists(filePath))
                {
                    logs.Add($" [✓] {agent.DirectoryName} required file: {requiredFile}");
                }
                else
                {
                    logs.Add($" [X] {agent.DirectoryName} required file missing: {requiredFile}");
                    missingRequiredFiles++;
                }
            }

            foreach (var mcp in validation.Manifest.RequiredMcpServers)
            {
                if (!mcp.Command.Equals("python", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var arg in mcp.Args)
                {
                    if (Path.IsPathRooted(arg))
                    {
                        if (!File.Exists(arg))
                        {
                            logs.Add($" [X] {agent.DirectoryName} MCP '{mcp.Name}' missing path: {arg}");
                            missingMcpPaths++;
                        }
                        else
                        {
                            logs.Add($" [✓] {agent.DirectoryName} MCP '{mcp.Name}' path: {arg}");
                        }

                        continue;
                    }

                    var normalizedArg = arg.Replace('/', Path.DirectorySeparatorChar);
                    var repoPath = Path.Combine(repoRootPath, normalizedArg);
                    if (!File.Exists(repoPath))
                    {
                        logs.Add($" [X] {agent.DirectoryName} MCP '{mcp.Name}' missing path: {arg}");
                        missingMcpPaths++;
                    }
                    else
                    {
                        logs.Add($" [✓] {agent.DirectoryName} MCP '{mcp.Name}' path: {arg}");
                    }
                }
            }

            foreach (var envVar in validation.Manifest.RequiredEnvVars)
            {
                requiredEnvVars.Add(envVar);
            }
        }

        var toolStatuses = await _toolInstallService.GetToolStatusesAsync(additionalTools);
        foreach (var status in toolStatuses)
        {
            var marker = status.IsAvailable ? "[✓]" : "[X]";
            var installHint = status.IsAvailable
                ? string.Empty
                : (status.CanAutoInstall && DeviceInfo.Platform == DevicePlatform.WinUI
                    ? " (auto-install available via winget during activation)"
                    : " (manual install required)");
            logs.Add($" {marker} {status.DisplayName} ({status.Command}){installHint}");
        }

        foreach (var envVar in requiredEnvVars.OrderBy(v => v, StringComparer.OrdinalIgnoreCase))
        {
            bool exists = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(envVar));
            logs.Add($" {(exists ? "[✓]" : "[X]")} ENV {envVar}{(exists ? string.Empty : " (not set)")}");
        }

        int missingTools = toolStatuses.Count(s => !s.IsAvailable);
        int missingEnv = requiredEnvVars.Count(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)));
        logs.Add("=============================");
        logs.Add(missingTools == 0 && missingEnv == 0 && missingRequiredFiles == 0 && missingMcpPaths == 0
            ? "[SYSTEM READY] Toolchain and environment requirements are satisfied."
            : $"[WARNING] Missing requirements detected: tools={missingTools}, env={missingEnv}, files={missingRequiredFiles}, mcpPaths={missingMcpPaths}.");

        return logs;
    }

    private static IReadOnlyList<string> GetPlatformSpecificNotes()
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return ["[*] Platform note: winget-backed auto-install is enabled during activation on Windows."];
        }

        return ["[*] Platform note: auto-install is disabled on this platform; install missing tools manually."];
    }
}
