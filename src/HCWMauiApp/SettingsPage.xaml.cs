using System.Diagnostics;
using System.Text.Json;

namespace WorkspaceManager;

public partial class SettingsPage : ContentPage
{
    private readonly string _settingsDir;
    private readonly string _settingsPath;

    public SettingsPage()
    {
        InitializeComponent();
        _settingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIArchitectAgents");
        _settingsPath = Path.Combine(_settingsDir, "app-preferences.json");
        Directory.CreateDirectory(_settingsDir);
        LoadPreferences();
    }

    private void LoadPreferences()
    {
        var prefs = AppPreferences.Default();
        if (File.Exists(_settingsPath))
        {
            try
            {
                string json = File.ReadAllText(_settingsPath);
                prefs = JsonSerializer.Deserialize<AppPreferences>(json) ?? prefs;
            }
            catch
            {
                SettingsStatusLabel.Text = "Could not read saved preferences; defaults are shown.";
            }
        }

        ThemePicker.SelectedItem = prefs.Theme;
        ShowHelperBalloonsSwitch.IsToggled = prefs.ShowHelperBalloons;
        LockPanelSizingSwitch.IsToggled = prefs.LockPanelSizing;
        RunAtStartupSwitch.IsToggled = prefs.RunAtStartup;
        ConfirmApplySwitch.IsToggled = prefs.ConfirmBeforeApply;
        AutoCheckUpdatesSwitch.IsToggled = prefs.CheckUpdatesOnScan;
        DashboardTrackCheckBox.IsChecked = prefs.InstallDashboardTrack;
        SqliteTrackCheckBox.IsChecked = prefs.InstallSqliteTrack;
        McpTrackCheckBox.IsChecked = prefs.InstallMcpTrack;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var prefs = new AppPreferences
        {
            Theme = ThemePicker.SelectedItem?.ToString() ?? "System",
            ShowHelperBalloons = ShowHelperBalloonsSwitch.IsToggled,
            LockPanelSizing = LockPanelSizingSwitch.IsToggled,
            RunAtStartup = RunAtStartupSwitch.IsToggled,
            ConfirmBeforeApply = ConfirmApplySwitch.IsToggled,
            CheckUpdatesOnScan = AutoCheckUpdatesSwitch.IsToggled,
            InstallDashboardTrack = DashboardTrackCheckBox.IsChecked,
            InstallSqliteTrack = SqliteTrackCheckBox.IsChecked,
            InstallMcpTrack = McpTrackCheckBox.IsChecked
        };

        string json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
        SettingsStatusLabel.Text = $"Saved preferences at {DateTime.Now:t}.";
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        bool reset = await DisplayAlertAsync(
            "Reset configuration",
            "Reset local app preferences to defaults? Downloaded packs and repository files are not deleted.",
            "Reset",
            "Cancel");

        if (!reset)
        {
            return;
        }

        if (File.Exists(_settingsPath))
        {
            File.Delete(_settingsPath);
        }

        LoadPreferences();
        SettingsStatusLabel.Text = "Configuration reset to defaults.";
    }

    private async void OnManualBackupClicked(object sender, EventArgs e)
    {
        string backupDir = Path.Combine(_settingsDir, "backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backupDir);

        if (File.Exists(_settingsPath))
        {
            File.Copy(_settingsPath, Path.Combine(backupDir, "app-preferences.json"), overwrite: true);
        }

        string optionalPrefs = Path.Combine(_settingsDir, "optional-feature-prefs.json");
        if (File.Exists(optionalPrefs))
        {
            File.Copy(optionalPrefs, Path.Combine(backupDir, "optional-feature-prefs.json"), overwrite: true);
        }

        SettingsStatusLabel.Text = $"Backup created: {backupDir}";
        await Task.CompletedTask;
    }

    private void OnOpenDataFolderClicked(object sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _settingsDir,
            UseShellExecute = true
        });
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private sealed class AppPreferences
    {
        public string Theme { get; set; } = "System";
        public bool ShowHelperBalloons { get; set; } = true;
        public bool LockPanelSizing { get; set; }
        public bool RunAtStartup { get; set; }
        public bool ConfirmBeforeApply { get; set; } = true;
        public bool CheckUpdatesOnScan { get; set; }
        public bool InstallDashboardTrack { get; set; } = true;
        public bool InstallSqliteTrack { get; set; } = true;
        public bool InstallMcpTrack { get; set; } = true;

        public static AppPreferences Default() => new();
    }
}