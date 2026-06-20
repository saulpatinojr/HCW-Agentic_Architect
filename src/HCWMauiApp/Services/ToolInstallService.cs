using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AgenticWorkspaceManager.Services;

public sealed class ToolInstallService
{
    private readonly IToolCommandExecutor _commandExecutor;
    private readonly IToolPlatform _toolPlatform;

    private sealed record ToolConfig(string DisplayName, string Command, string? WingetId);

    private static readonly IReadOnlyList<ToolConfig> RequiredTools =
    [
        new("AWS CLI", "aws", "Amazon.AWSCLI"),
        new("Azure CLI", "az", "Microsoft.AzureCLI"),
        new("GCP CLI", "gcloud", null),
        new("Terraform", "terraform", "Hashicorp.Terraform"),
        new("Ansible", "ansible", null),
        new("VMware CLI", "vmware", null),
        new("Kubernetes", "kubectl", "Kubernetes.kubectl"),
        new("Helm", "helm", "Helm.Helm"),
        new("Docker", "docker", "Docker.DockerDesktop"),
        new("GitHub CLI", "gh", "GitHub.cli"),
        new("Claude CLI", "claude", null),
        new("Codex CLI", "codex", null)
    ];

    public ToolInstallService()
        : this(new ProcessToolCommandExecutor(), new DefaultToolPlatform())
    {
    }

    public ToolInstallService(IToolCommandExecutor commandExecutor, IToolPlatform toolPlatform)
    {
        _commandExecutor = commandExecutor;
        _toolPlatform = toolPlatform;
    }

    public IReadOnlyList<string> GetMissingTools()
    {
        return GetMissingTools(null);
    }

    public IReadOnlyList<string> GetMissingTools(IEnumerable<ManifestToolRequirement>? additionalTools)
    {
        var missing = new List<string>();
        var statuses = GetToolStatusesAsync(additionalTools).GetAwaiter().GetResult();
        foreach (var status in statuses)
        {
            if (!status.IsAvailable)
            {
                missing.Add(status.Command);
            }
        }

        return missing;
    }

    public IReadOnlyList<string> GetToolStatusLabels()
    {
        return GetToolStatusLabels(null);
    }

    public IReadOnlyList<string> GetToolStatusLabels(IEnumerable<ManifestToolRequirement>? additionalTools)
    {
        var statuses = GetToolStatusesAsync(additionalTools).GetAwaiter().GetResult();
        var lines = new List<string>();
        foreach (var status in statuses)
        {
            lines.Add($"{(status.IsAvailable ? "[✓]" : "[X]")} {status.DisplayName} ({status.Command})");
        }

        return lines;
    }

    public async Task<IReadOnlyList<ToolCheckStatus>> GetToolStatusesAsync(IEnumerable<ManifestToolRequirement>? additionalTools)
    {
        var statuses = new List<ToolCheckStatus>();
        var effectiveTools = BuildEffectiveToolList(additionalTools);

        foreach (var tool in effectiveTools)
        {
            var exists = await _commandExecutor.IsCommandAvailable(tool.Command, _toolPlatform.IsWindowsPlatform);
            statuses.Add(new ToolCheckStatus(tool.DisplayName, tool.Command, exists, !string.IsNullOrWhiteSpace(tool.WingetId), tool.WingetId));
        }

        var ghAvailable = statuses.FirstOrDefault(s => s.Command.Equals("gh", StringComparison.OrdinalIgnoreCase))?.IsAvailable == true;
        bool ghCopilotAvailable = ghAvailable && await _commandExecutor.IsCommandSubcommandAvailable("gh", "copilot");
        statuses.Add(new ToolCheckStatus("GitHub Copilot CLI", "gh copilot", ghCopilotAvailable, false, null));

        return statuses;
    }

    public async Task<IReadOnlyList<string>> InstallMissingToolsAsync()
    {
        return await InstallMissingToolsAsync(null);
    }

    public async Task<IReadOnlyList<string>> InstallMissingToolsAsync(IEnumerable<ManifestToolRequirement>? additionalTools)
    {
        var logs = new List<string>();
        var effectiveTools = BuildEffectiveToolList(additionalTools);

        if (!_toolPlatform.IsWindowsPlatform)
        {
            logs.Add("[!] Winget auto-install is only enabled on Windows.");
            return logs;
        }

        foreach (var tool in effectiveTools)
        {
            if (await _commandExecutor.IsCommandAvailable(tool.Command, _toolPlatform.IsWindowsPlatform))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(tool.WingetId))
            {
                logs.Add($"[-] {tool.DisplayName}: no winget package mapping, manual install required.");
                continue;
            }

            var installed = await _commandExecutor.InstallWithWingetAsync(tool.WingetId!);
            logs.Add(installed
                ? $"[+] {tool.DisplayName}: installed via winget."
                : $"[-] {tool.DisplayName}: winget install failed.");
        }

        return logs;
    }

    private static IReadOnlyList<ToolConfig> BuildEffectiveToolList(IEnumerable<ManifestToolRequirement>? additionalTools)
    {
        var merged = RequiredTools
            .ToDictionary(
                t => t.Command,
                t => t,
                StringComparer.OrdinalIgnoreCase);

        if (additionalTools is null)
        {
            return merged.Values.ToList();
        }

        foreach (var extra in additionalTools)
        {
            if (string.IsNullOrWhiteSpace(extra.Command))
            {
                continue;
            }

            if (!merged.TryGetValue(extra.Command, out var existing))
            {
                merged[extra.Command] = new ToolConfig(
                    string.IsNullOrWhiteSpace(extra.DisplayName) ? extra.Command : extra.DisplayName,
                    extra.Command,
                    string.IsNullOrWhiteSpace(extra.WingetId) ? null : extra.WingetId);
                continue;
            }

            if (string.IsNullOrWhiteSpace(existing.WingetId) && !string.IsNullOrWhiteSpace(extra.WingetId))
            {
                merged[extra.Command] = existing with { WingetId = extra.WingetId };
            }
        }

        return merged.Values.ToList();
    }
}

public sealed class ToolCheckStatus
{
    public ToolCheckStatus(string displayName, string command, bool isAvailable, bool canAutoInstall, string? wingetId)
    {
        DisplayName = displayName;
        Command = command;
        IsAvailable = isAvailable;
        CanAutoInstall = canAutoInstall;
        WingetId = wingetId;
    }

    public string DisplayName { get; }
    public string Command { get; }
    public bool IsAvailable { get; }
    public bool CanAutoInstall { get; }
    public string? WingetId { get; }
}

public interface IToolPlatform
{
    bool IsWindowsPlatform { get; }
}

public sealed class DefaultToolPlatform : IToolPlatform
{
    public bool IsWindowsPlatform => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}

public interface IToolCommandExecutor
{
    Task<bool> IsCommandAvailable(string command, bool isWindows);
    Task<bool> IsCommandSubcommandAvailable(string command, string subcommand);
    Task<bool> InstallWithWingetAsync(string wingetId);
}

public sealed class ProcessToolCommandExecutor : IToolCommandExecutor
{
    public async Task<bool> IsCommandAvailable(string command, bool isWindows)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = isWindows ? "cmd" : "bash",
                    Arguments = isWindows ? $"/c where {command}" : $"-c \"which {command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsCommandSubcommandAvailable(string command, string subcommand)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = $"{subcommand} --help",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InstallWithWingetAsync(string wingetId)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = $"install --id {wingetId} --accept-package-agreements --accept-source-agreements --silent",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}