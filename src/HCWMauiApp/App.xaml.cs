namespace WorkspaceManager;

public partial class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_mainPage)
        {
            Title = "AI Architect Agents",
            Width = 1420,
            Height = 900,
            MinimumWidth = 1180,
            MinimumHeight = 760
        };
    }
}
