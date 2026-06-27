using System.Diagnostics;
using WorkspaceManager.Services;

namespace WorkspaceManager;

public partial class SettingsPage : ContentPage
{
    private readonly AppPreferenceStore _preferenceStore;
    private readonly string _settingsDir;
    private readonly string _workspaceDataDir;

    public SettingsPage()
    {
        InitializeComponent();
        _preferenceStore = new AppPreferenceStore();
        _settingsDir = _preferenceStore.SettingsDirectory;
        _workspaceDataDir = ResolveWorkspaceDataDirectory();
        Directory.CreateDirectory(_settingsDir);
        LoadPreferences();
    }

    private static string ResolveWorkspaceDataDirectory()
    {
        var catalogService = new WorkspaceCatalogService();
        string repoRoot = catalogService.DetermineRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
        string workspaceConfigPath = Path.Combine(repoRoot, "workspace-config");
        return Directory.Exists(workspaceConfigPath) ? workspaceConfigPath : repoRoot;
    }

    private void LoadPreferences()
    {
        var prefs = _preferenceStore.Load();

        ThemePicker.SelectedItem = prefs.Theme;
        ShowHelperBalloonsSwitch.IsToggled = prefs.ShowHelperBalloons;
        LockPanelSizingSwitch.IsToggled = prefs.LockPanelSizing;
        RunAtStartupSwitch.IsToggled = prefs.RunAtStartup;
        ConfirmApplySwitch.IsToggled = prefs.ConfirmBeforeApply;
        AutoCheckUpdatesSwitch.IsToggled = prefs.CheckUpdatesOnScan;
        PreviewModeDefaultSwitch.IsToggled = prefs.PreviewModeDefault;
        EnableHelperMcpSwitch.IsToggled = prefs.EnableHelperMcp;
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
            PreviewModeDefault = PreviewModeDefaultSwitch.IsToggled,
            EnableHelperMcp = EnableHelperMcpSwitch.IsToggled,
            InstallDashboardTrack = DashboardTrackCheckBox.IsChecked,
            InstallSqliteTrack = SqliteTrackCheckBox.IsChecked,
            InstallMcpTrack = McpTrackCheckBox.IsChecked
        };

        await _preferenceStore.SaveAsync(prefs);
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

        string settingsPath = Path.Combine(_settingsDir, "app-preferences.json");
        if (File.Exists(settingsPath))
        {
            File.Delete(settingsPath);
        }

        LoadPreferences();
        SettingsStatusLabel.Text = "Configuration reset to defaults.";
    }

    private async void OnManualBackupClicked(object sender, EventArgs e)
    {
        string backupDir = Path.Combine(_settingsDir, "backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backupDir);

        string settingsPath = Path.Combine(_settingsDir, "app-preferences.json");

        if (File.Exists(settingsPath))
        {
            File.Copy(settingsPath, Path.Combine(backupDir, "app-preferences.json"), overwrite: true);
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
            FileName = _workspaceDataDir,
            UseShellExecute = true
        });
        SettingsStatusLabel.Text = $"Opened: {_workspaceDataDir}";
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}