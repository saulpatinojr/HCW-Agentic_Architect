using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class AppPreferenceStore
{
    private readonly string _settingsDir;
    private readonly string _settingsPath;

    public AppPreferenceStore()
    {
        _settingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIArchitectAgents");
        _settingsPath = Path.Combine(_settingsDir, "app-preferences.json");
    }

    public string SettingsDirectory => _settingsDir;

    public AppPreferences Load()
    {
        Directory.CreateDirectory(_settingsDir);

        var prefs = AppPreferences.Default();
        if (!File.Exists(_settingsPath))
        {
            return prefs;
        }

        try
        {
            string json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppPreferences>(json) ?? prefs;
        }
        catch
        {
            return prefs;
        }
    }

    public async Task SaveAsync(AppPreferences prefs)
    {
        Directory.CreateDirectory(_settingsDir);
        string json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
}

public sealed class AppPreferences
{
    public string Theme { get; set; } = "System";
    public bool ShowHelperBalloons { get; set; } = true;
    public bool LockPanelSizing { get; set; }
    public bool RunAtStartup { get; set; }
    public bool ConfirmBeforeApply { get; set; } = true;
    public bool CheckUpdatesOnScan { get; set; }
    public bool PreviewModeDefault { get; set; } = true;
    public bool EnableHelperMcp { get; set; } = true;
    public bool InstallDashboardTrack { get; set; } = true;
    public bool InstallSqliteTrack { get; set; } = true;
    public bool InstallMcpTrack { get; set; } = true;

    public static AppPreferences Default() => new();
}
