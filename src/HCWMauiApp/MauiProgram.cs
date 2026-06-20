namespace AgenticWorkspaceManager;

using AgenticWorkspaceManager.Services;

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
        builder.Services.AddSingleton<WorkspaceCatalogService>();
        builder.Services.AddSingleton<WorkspaceMcpConfigBuilderService>();
        builder.Services.AddSingleton<WorkspaceWriterService>();
        builder.Services.AddSingleton<WorkspaceSystemCheckService>();
        builder.Services.AddSingleton<TeamAssemblyService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}