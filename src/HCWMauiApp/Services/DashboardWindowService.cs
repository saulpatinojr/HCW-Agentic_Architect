namespace WorkspaceManager.Services;

public sealed class DashboardWindowService
{
    public void OpenDashboardWindow(ContentPage page)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.OpenWindow(new Window(page)
        {
            Title = "Context Optimization Dashboard",
            Width = 1120,
            Height = 760,
            MinimumWidth = 900,
            MinimumHeight = 620
        });
    }
}
