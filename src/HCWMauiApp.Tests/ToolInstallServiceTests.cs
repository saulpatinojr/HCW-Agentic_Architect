using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class ToolInstallServiceTests
{
    [Fact]
    public async Task GetToolStatusesAsync_WhenGhAndCopilotAreAvailable_IncludesCopilotAsAvailable()
    {
        var executor = new FakeToolCommandExecutor();
        executor.AvailableCommands.Add("gh");
        executor.AvailableSubcommands.Add("gh|copilot");

        var service = new ToolInstallService(executor, new FakeToolPlatform(isWindows: true));

        var statuses = await service.GetToolStatusesAsync(null);

        var copilot = Assert.Single(statuses.Where(s => s.Command == "gh copilot"));
        Assert.True(copilot.IsAvailable);
    }

    [Fact]
    public async Task InstallMissingToolsAsync_WhenNotWindows_ReturnsWingetDisabledMessage()
    {
        var service = new ToolInstallService(new FakeToolCommandExecutor(), new FakeToolPlatform(isWindows: false));

        var logs = await service.InstallMissingToolsAsync();

        Assert.Contains("[!] Winget auto-install is only enabled on Windows.", logs);
    }

    [Fact]
    public async Task InstallMissingToolsAsync_WhenExistingToolReceivesWingetMapping_UsesProvidedWingetId()
    {
        var executor = new FakeToolCommandExecutor();
        MarkAllBaselineCommandsAsAvailable(executor);

        // Force baseline gcloud to be missing so added winget mapping is exercised.
        executor.AvailableCommands.Remove("gcloud");
        executor.WingetInstallResults["Google.CloudSDK"] = true;

        var service = new ToolInstallService(executor, new FakeToolPlatform(isWindows: true));

        var logs = await service.InstallMissingToolsAsync(
        [
            new ManifestToolRequirement
            {
                DisplayName = "Google Cloud CLI",
                Command = "gcloud",
                WingetId = "Google.CloudSDK"
            }
        ]);

        Assert.Contains("[+] GCP CLI: installed via winget.", logs);
        Assert.Contains("Google.CloudSDK", executor.InstalledWingetIds);
    }

    [Fact]
    public async Task InstallMissingToolsAsync_WhenAdditionalToolHasNoWingetMapping_ReportsManualInstall()
    {
        var executor = new FakeToolCommandExecutor();
        MarkAllBaselineCommandsAsAvailable(executor);

        // Additional tool is missing and has no mapping.
        var service = new ToolInstallService(executor, new FakeToolPlatform(isWindows: true));

        var logs = await service.InstallMissingToolsAsync(
        [
            new ManifestToolRequirement
            {
                DisplayName = "Custom Tool",
                Command = "customtool",
                WingetId = ""
            }
        ]);

        Assert.Contains("[-] Custom Tool: no winget package mapping, manual install required.", logs);
    }

    [Fact]
    public async Task InstallMissingToolsAsync_WhenAdditionalToolHasWingetMapping_InstallsThroughExecutor()
    {
        var executor = new FakeToolCommandExecutor();
        MarkAllBaselineCommandsAsAvailable(executor);
        executor.WingetInstallResults["Contoso.CustomTool"] = true;

        var service = new ToolInstallService(executor, new FakeToolPlatform(isWindows: true));

        var logs = await service.InstallMissingToolsAsync(
        [
            new ManifestToolRequirement
            {
                DisplayName = "Custom Tool",
                Command = "customtool",
                WingetId = "Contoso.CustomTool"
            }
        ]);

        Assert.Contains("[+] Custom Tool: installed via winget.", logs);
        Assert.Contains("Contoso.CustomTool", executor.InstalledWingetIds);
    }

    private static void MarkAllBaselineCommandsAsAvailable(FakeToolCommandExecutor executor)
    {
        foreach (var cmd in new[]
                 {
                     "aws", "az", "gcloud", "terraform", "ansible", "vmware", "kubectl", "helm", "docker", "gh", "claude", "codex"
                 })
        {
            executor.AvailableCommands.Add(cmd);
        }
    }

    private sealed class FakeToolPlatform : IToolPlatform
    {
        public FakeToolPlatform(bool isWindows)
        {
            IsWindowsPlatform = isWindows;
        }

        public bool IsWindowsPlatform { get; }
    }

    private sealed class FakeToolCommandExecutor : IToolCommandExecutor
    {
        public HashSet<string> AvailableCommands { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> AvailableSubcommands { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> WingetInstallResults { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> InstalledWingetIds { get; } = [];

        public Task<bool> IsCommandAvailable(string command, bool isWindows)
        {
            return Task.FromResult(AvailableCommands.Contains(command));
        }

        public Task<bool> IsCommandSubcommandAvailable(string command, string subcommand)
        {
            return Task.FromResult(AvailableSubcommands.Contains($"{command}|{subcommand}"));
        }

        public Task<bool> InstallWithWingetAsync(string wingetId)
        {
            InstalledWingetIds.Add(wingetId);
            bool result = WingetInstallResults.TryGetValue(wingetId, out var success) && success;
            return Task.FromResult(result);
        }
    }
}
