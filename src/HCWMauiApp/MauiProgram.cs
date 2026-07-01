namespace WorkspaceManager;

using WorkspaceManager.Services;
using WorkspaceManager.Features.ContextOptimization;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<ToolInstallService>();
        builder.Services.AddSingleton<PackManifestService>();
        builder.Services.AddSingleton<ManifestRequirementsMergeService>();
        builder.Services.AddSingleton<WorkspacePackCatalogService>();
        builder.Services.AddSingleton<WorkspaceCatalogService>();
        builder.Services.AddSingleton<ProviderRegistryService>();
        builder.Services.AddSingleton<WorkspaceFolderService>();
        builder.Services.AddSingleton<HelperMcpHealthService>();
        builder.Services.AddSingleton<WorkspacePackUpdateService>();
        builder.Services.AddSingleton<WorkspaceMcpConfigBuilderService>();
        builder.Services.AddSingleton<WorkspaceWriterService>();
        builder.Services.AddSingleton<WorkspacePolicyService>();
        builder.Services.AddSingleton<WorkspaceSystemCheckService>();
        builder.Services.AddSingleton<WorkspaceActivationService>();
        builder.Services.AddSingleton<OptionalFeatureSetupService>();
        builder.Services.AddSingleton<ContextOptimizationHistoryStore>();
        builder.Services.AddSingleton<PartnerAdapterHealthService>();
        builder.Services.AddSingleton<ContextOptimizationExportService>();
        builder.Services.AddSingleton<ContextOptimizationMetricsService>();
        builder.Services.AddSingleton<DashboardWindowService>();
        builder.Services.AddTransient<ContextOptimizationDashboardPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}
