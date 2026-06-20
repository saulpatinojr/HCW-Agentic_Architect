using System.Diagnostics;

namespace AgenticWorkspaceManager.Services;

public sealed class ToolInstallService
{
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

    public IReadOnlyList<string> GetMissingTools()
    {
        var missing = new List<string>();
        foreach (var tool in RequiredTools)
        {
            if (!IsCommandAvailable(tool.Command).GetAwaiter().GetResult())
            {
                missing.Add(tool.Command);
            }
        }

        return missing;
    }

    public IReadOnlyList<string> GetToolStatusLabels()
    {
        var lines = new List<string>();
        foreach (var tool in RequiredTools)
        {
            var exists = IsCommandAvailable(tool.Command).GetAwaiter().GetResult();
            lines.Add($"{(exists ? "[✓]" : "[X]")} {tool.DisplayName} ({tool.Command})");
        }

        return lines;
    }

    public async Task<IReadOnlyList<string>> InstallMissingToolsAsync()
    {
        var logs = new List<string>();

        if (DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            logs.Add("[!] Winget auto-install is only enabled on Windows.");
            return logs;
        }

        foreach (var tool in RequiredTools)
        {
            if (await IsCommandAvailable(tool.Command))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(tool.WingetId))
            {
                logs.Add($"[-] {tool.DisplayName}: no winget package mapping, manual install required.");
                continue;
            }

            var installed = await InstallWithWingetAsync(tool);
            logs.Add(installed
                ? $"[+] {tool.DisplayName}: installed via winget."
                : $"[-] {tool.DisplayName}: winget install failed.");
        }

        return logs;
    }

    private static async Task<bool> InstallWithWingetAsync(ToolConfig tool)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = $"install --id {tool.WingetId} --accept-package-agreements --accept-source-agreements --silent",
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

    private static async Task<bool> IsCommandAvailable(string command)
    {
        try
        {
            bool isWindows = DeviceInfo.Platform == DevicePlatform.WinUI;
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
}