using System.Diagnostics;
using WorkspaceManager.Services;

namespace WorkspaceManager;

public partial class SettingsPage : ContentPage
{
    private readonly AppPreferenceStore _preferenceStore;
    private readonly WorkspacePolicyService _policyService;
    private readonly string _settingsDir;
    private readonly string _workspaceDataDir;
    private WorkspacePolicyManifest _policyManifest = WorkspacePolicyManifest.Default();

    public SettingsPage()
    {
        InitializeComponent();
        _preferenceStore = new AppPreferenceStore();
        _policyService = new WorkspacePolicyService();
        _settingsDir = _preferenceStore.SettingsDirectory;
        _workspaceDataDir = ResolveWorkspaceDataDirectory();
        Directory.CreateDirectory(_settingsDir);
        LoadPreferences();
        LoadPolicyPreferences();
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

    private void LoadPolicyPreferences()
    {
        _policyManifest = _policyService.Load();

        var securityProfiles = _policyService.GetSecurityProfiles().ToList();
        SecurityProfilePicker.ItemsSource = securityProfiles;
        SecurityProfilePicker.ItemDisplayBinding = new Binding(nameof(SecurityProfileDefinition.DisplayName));
        SecurityProfilePicker.SelectedItem = securityProfiles.FirstOrDefault(profile => profile.Name.Equals(_policyManifest.SecurityProfile, StringComparison.OrdinalIgnoreCase)) ?? securityProfiles.First();

        var workflowBundles = _policyService.GetWorkflowBundles().ToList();
        WorkflowBundlePicker.ItemsSource = workflowBundles;
        WorkflowBundlePicker.ItemDisplayBinding = new Binding(nameof(WorkflowBundleDefinition.DisplayName));
        WorkflowBundlePicker.SelectedItem = workflowBundles.FirstOrDefault(bundle => bundle.Name.Equals(_policyManifest.WorkflowBundle, StringComparison.OrdinalIgnoreCase)) ?? workflowBundles.First();

        var delegationModes = _policyService.GetDelegationModes().ToList();
        DelegationModePicker.ItemsSource = delegationModes;
        DelegationModePicker.ItemDisplayBinding = new Binding(nameof(DelegationModeDefinition.DisplayName));
        DelegationModePicker.SelectedItem = delegationModes.FirstOrDefault(mode => mode.Name.Equals(_policyManifest.DelegationMode, StringComparison.OrdinalIgnoreCase)) ?? delegationModes.First();

        StrictWorkflowModeSwitch.IsToggled = _policyManifest.StrictWorkflowMode;
        ResearchScoreMinEntry.Text = _policyManifest.ResearchScoreMin.ToString();
        PlanScoreMinEntry.Text = _policyManifest.PlanScoreMin.ToString();
        TestPassRequiredSwitch.IsToggled = _policyManifest.TestPassRequired;
        MaxAutoRetryEntry.Text = _policyManifest.MaxAutoRetry.ToString();
        BypassReasonEntry.Text = _policyManifest.BypassReason;
        DelegationProfilesEditor.Text = string.IsNullOrWhiteSpace(_policyManifest.DelegationProfilesJson)
            ? _policyService.GetDefaultDelegationProfilesJson()
            : _policyManifest.DelegationProfilesJson;

        PolicyStatusLabel.Text = $"Loaded {_policyManifest.SecurityProfile} / {_policyManifest.WorkflowBundle} / {_policyManifest.DelegationMode}.";
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

    private async void OnSavePolicyVariantClicked(object sender, EventArgs e)
    {
        if (SecurityProfilePicker.SelectedItem is not SecurityProfileDefinition securityProfile ||
            WorkflowBundlePicker.SelectedItem is not WorkflowBundleDefinition workflowBundle ||
            DelegationModePicker.SelectedItem is not DelegationModeDefinition delegationMode)
        {
            PolicyStatusLabel.Text = "Select a valid policy preset before saving.";
            return;
        }

        if (!int.TryParse(ResearchScoreMinEntry.Text, out int researchMin))
        {
            researchMin = 70;
        }

        if (!int.TryParse(PlanScoreMinEntry.Text, out int planMin))
        {
            planMin = 75;
        }

        if (!int.TryParse(MaxAutoRetryEntry.Text, out int maxAutoRetry))
        {
            maxAutoRetry = 2;
        }

        _policyManifest = new WorkspacePolicyManifest
        {
            SecurityProfile = securityProfile.Name,
            WorkflowBundle = workflowBundle.Name,
            DelegationMode = delegationMode.Name,
            StrictWorkflowMode = StrictWorkflowModeSwitch.IsToggled,
            ResearchScoreMin = researchMin,
            PlanScoreMin = planMin,
            TestPassRequired = TestPassRequiredSwitch.IsToggled,
            MaxAutoRetry = maxAutoRetry,
            BypassReason = BypassReasonEntry.Text ?? string.Empty,
            DelegationProfilesJson = string.IsNullOrWhiteSpace(DelegationProfilesEditor.Text)
                ? _policyService.GetDefaultDelegationProfilesJson()
                : DelegationProfilesEditor.Text
        };

        await _policyService.SaveAsync(_policyManifest);
        PolicyStatusLabel.Text = $"Saved local policy variant at {DateTime.Now:t}.";
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

        string policyPrefs = Path.Combine(_settingsDir, "policy-manifest.json");
        if (File.Exists(policyPrefs))
        {
            File.Copy(policyPrefs, Path.Combine(backupDir, "policy-manifest.json"), overwrite: true);
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
