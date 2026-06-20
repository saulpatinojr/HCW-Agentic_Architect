namespace WorkspaceManager.Services;

public sealed class HelperMcpHealthService
{
    private readonly IToolCommandExecutor _commandExecutor;
    private readonly IToolPlatform _toolPlatform;

    public HelperMcpHealthService()
        : this(new ProcessToolCommandExecutor(), new DefaultToolPlatform())
    {
    }

    public HelperMcpHealthService(IToolCommandExecutor commandExecutor, IToolPlatform toolPlatform)
    {
        _commandExecutor = commandExecutor;
        _toolPlatform = toolPlatform;
    }

    public async Task<HelperMcpHealth> CheckAsync(string repoRootPath, bool isLinked)
    {
        string scriptPath = Path.Combine(repoRootPath, "workspace-config", "mcp-servers", "token-compressor", "server.py");
        bool scriptExists = File.Exists(scriptPath);
        bool pythonAvailable = await _commandExecutor.IsCommandAvailable("python", _toolPlatform.IsWindowsPlatform);
        bool packageCheckPassed = scriptExists && File.ReadAllText(scriptPath).Contains("FastMCP", StringComparison.Ordinal);

        return new HelperMcpHealth
        {
            ScriptExists = scriptExists,
            PythonAvailable = pythonAvailable,
            PackageCheckPassed = packageCheckPassed,
            IsLinked = isLinked,
            ScriptPath = scriptPath
        };
    }
}
