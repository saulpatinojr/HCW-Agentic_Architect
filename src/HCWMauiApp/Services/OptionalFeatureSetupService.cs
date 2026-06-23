using System.Diagnostics;
using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class OptionalFeatureSetupService
{
    private readonly string _prefsPath;

    public OptionalFeatureSetupService()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dir = Path.Combine(root, "AIArchitectAgents");
        Directory.CreateDirectory(dir);
        _prefsPath = Path.Combine(dir, "optional-feature-prefs.json");
    }

    public async Task<OptionalFeaturePromptState> EvaluateAsync(string repoRootPath)
    {
        var prefs = await LoadPreferencesAsync();
        bool mcpInstalled = await IsMcpTrackInstalledAsync();

        var tracks = new List<OptionalFeatureTrackState>
        {
            new()
            {
                Key = "dashboard",
                DisplayName = "Dashboard",
                IsInstalled = true,
                IsSelected = prefs.SelectedTracks.Contains("dashboard"),
                Detail = "Bundled in app build"
            },
            new()
            {
                Key = "sqlite",
                DisplayName = "SQLite trend store",
                IsInstalled = true,
                IsSelected = prefs.SelectedTracks.Contains("sqlite"),
                Detail = "Bundled in app build"
            },
            new()
            {
                Key = "mcp",
                DisplayName = "MCP context optimization runtime",
                IsInstalled = mcpInstalled,
                IsSelected = prefs.SelectedTracks.Contains("mcp"),
                Detail = mcpInstalled
                    ? "Python packages detected"
                    : "Requires python dependencies from requirements.txt"
            }
        };

        bool shouldPrompt = !prefs.NeverAskAgain && tracks.Any(t => !t.IsInstalled);

        return new OptionalFeaturePromptState
        {
            NeverAskAgain = prefs.NeverAskAgain,
            ShouldPrompt = shouldPrompt,
            Tracks = tracks
        };
    }

    public async Task<OptionalFeatureInstallResult> InstallAsync(
        string repoRootPath,
        OptionalFeatureInstallRequest request)
    {
        var logs = new List<string>();
        bool success = true;

        var selectedTracks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (request.InstallDashboard)
        {
            selectedTracks.Add("dashboard");
            logs.Add("[+] Dashboard track is bundled; no extra installation required.");
        }

        if (request.InstallSqlite)
        {
            selectedTracks.Add("sqlite");
            logs.Add("[+] SQLite track is bundled; no extra installation required.");
        }

        if (request.InstallMcp)
        {
            selectedTracks.Add("mcp");
            bool alreadyInstalled = await IsMcpTrackInstalledAsync();
            if (alreadyInstalled)
            {
                logs.Add("[+] MCP track already installed; skipping dependency install.");
            }
            else
            {
                string requirementsPath = Path.Combine(
                    repoRootPath,
                    "workspace-config",
                    "mcp-servers",
                    "token-compressor",
                    "requirements.txt");

                if (!File.Exists(requirementsPath))
                {
                    logs.Add("[-] MCP track requirements.txt not found.");
                    success = false;
                }
                else
                {
                    var result = await RunProcessAsync(
                        "python",
                        $"-m pip install -r \"{requirementsPath}\"");

                    if (result.ExitCode == 0)
                    {
                        logs.Add("[+] MCP track dependencies installed from requirements.txt.");
                    }
                    else
                    {
                        logs.Add("[-] MCP dependency install failed.");
                        if (!string.IsNullOrWhiteSpace(result.Error))
                        {
                            logs.Add($"[-] pip error: {result.Error.Trim()}");
                        }

                        success = false;
                    }
                }
            }
        }

        await SavePreferencesAsync(new OptionalFeaturePreferences
        {
            NeverAskAgain = request.NeverAskAgain,
            SelectedTracks = selectedTracks
        });

        var state = await EvaluateAsync(repoRootPath);
        return new OptionalFeatureInstallResult
        {
            Succeeded = success,
            Logs = logs,
            State = state
        };
    }

    public async Task SetNeverAskAgainAsync(bool neverAskAgain)
    {
        var prefs = await LoadPreferencesAsync();
        prefs.NeverAskAgain = neverAskAgain;
        await SavePreferencesAsync(prefs);
    }

    private async Task<bool> IsMcpTrackInstalledAsync()
    {
        var result = await RunProcessAsync("python", "-c \"import mcp, headroom\"");
        return result.ExitCode == 0;
    }

    private async Task<OptionalFeaturePreferences> LoadPreferencesAsync()
    {
        if (!File.Exists(_prefsPath))
        {
            return OptionalFeaturePreferences.Default();
        }

        try
        {
            string json = await File.ReadAllTextAsync(_prefsPath);
            var prefs = JsonSerializer.Deserialize<OptionalFeaturePreferences>(json);
            return prefs ?? OptionalFeaturePreferences.Default();
        }
        catch
        {
            return OptionalFeaturePreferences.Default();
        }
    }

    private async Task SavePreferencesAsync(OptionalFeaturePreferences prefs)
    {
        string json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_prefsPath, json);
    }

    private static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new ProcessResult(process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            return new ProcessResult(-1, string.Empty, ex.Message);
        }
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}

public sealed class OptionalFeatureInstallRequest
{
    public bool InstallDashboard { get; set; }
    public bool InstallSqlite { get; set; }
    public bool InstallMcp { get; set; }
    public bool NeverAskAgain { get; set; }
}

public sealed class OptionalFeatureInstallResult
{
    public bool Succeeded { get; set; }
    public IReadOnlyList<string> Logs { get; set; } = [];
    public OptionalFeaturePromptState State { get; set; } = new();
}

public sealed class OptionalFeaturePromptState
{
    public bool ShouldPrompt { get; set; }
    public bool NeverAskAgain { get; set; }
    public IReadOnlyList<OptionalFeatureTrackState> Tracks { get; set; } = [];
}

public sealed class OptionalFeatureTrackState
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsInstalled { get; set; }
    public bool IsSelected { get; set; }
    public string Detail { get; set; } = string.Empty;
}

public sealed class OptionalFeaturePreferences
{
    public bool NeverAskAgain { get; set; }
    public HashSet<string> SelectedTracks { get; set; } = [];

    public static OptionalFeaturePreferences Default()
    {
        return new OptionalFeaturePreferences
        {
            NeverAskAgain = false,
            SelectedTracks = ["dashboard", "sqlite", "mcp"]
        };
    }
}
