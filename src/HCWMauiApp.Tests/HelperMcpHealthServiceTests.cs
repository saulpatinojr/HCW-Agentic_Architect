using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class HelperMcpHealthServiceTests : IDisposable
{
    private readonly string _repoRoot;
    private readonly FakeToolCommandExecutor _executor = new();

    public HelperMcpHealthServiceTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "hcw-helper-mcp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_repoRoot);
    }

    [Fact]
    public async Task CheckAsync_WhenScriptMissing_ReturnsNeedsAttention()
    {
        var service = new HelperMcpHealthService(_executor, new FakeToolPlatform(true));

        var result = await service.CheckAsync(_repoRoot, isLinked: true);

        Assert.False(result.ScriptExists);
        Assert.Equal("Helper needs attention", result.Status);
    }

    [Fact]
    public async Task CheckAsync_WhenScriptAndPythonAvailable_ReturnsReady()
    {
        _executor.AvailableCommands.Add("python");
        string helperDir = Path.Combine(_repoRoot, "workspace-config", "mcp-servers", "token-compressor");
        Directory.CreateDirectory(helperDir);
        File.WriteAllText(Path.Combine(helperDir, "server.py"), "from mcp.server.fastmcp import FastMCP");
        var service = new HelperMcpHealthService(_executor, new FakeToolPlatform(true));

        var result = await service.CheckAsync(_repoRoot, isLinked: true);

        Assert.True(result.ScriptExists);
        Assert.True(result.PythonAvailable);
        Assert.True(result.PackageCheckPassed);
        Assert.True(result.IsLinked);
        Assert.Equal("Helper ready", result.Status);
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
        {
            Directory.Delete(_repoRoot, true);
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

        public Task<bool> IsCommandAvailable(string command, bool isWindows)
        {
            return Task.FromResult(AvailableCommands.Contains(command));
        }

        public Task<bool> IsCommandSubcommandAvailable(string command, string subcommand)
        {
            return Task.FromResult(false);
        }

        public Task<bool> InstallWithWingetAsync(string wingetId)
        {
            return Task.FromResult(true);
        }
    }
}
