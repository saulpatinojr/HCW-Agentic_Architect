using System.Diagnostics;

namespace WorkspaceManager.Services;

public sealed class PartnerAdapterHealthService
{
    private readonly bool _isWindows = OperatingSystem.IsWindows();

    public IReadOnlyList<PartnerAdapterHealthItem> GetHealth()
    {
        var checks = new List<(string Partner, string Command)>
        {
            ("codex", "codex"),
            ("claude", "claude"),
            ("github", "gh"),
            ("copilot", "gh"),
            ("antigravity", "antigravity"),
            ("vscode", "code")
        };

        var items = new List<PartnerAdapterHealthItem>();
        foreach (var (partner, command) in checks)
        {
            bool available = IsCommandAvailable(command);
            string detail = available
                ? $"{command} detected on PATH"
                : $"{command} not found on PATH";

            if (partner == "copilot" && available)
            {
                detail = IsGhCopilotAvailable()
                    ? "gh copilot extension available"
                    : "gh found; gh copilot extension missing";
                available = IsGhCopilotAvailable();
            }

            items.Add(new PartnerAdapterHealthItem
            {
                Partner = partner,
                IsHealthy = available,
                Status = available ? "Healthy" : "Needs setup",
                Detail = detail
            });
        }

        return items;
    }

    private bool IsCommandAvailable(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _isWindows ? "cmd" : "bash",
                    Arguments = _isWindows ? $"/c where {command}" : $"-c \"which {command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    private bool IsGhCopilotAvailable()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gh",
                    Arguments = "copilot --help",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(2500);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
