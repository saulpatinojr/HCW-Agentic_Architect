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
            Width = 1040,
            Height = 720,
            MinimumWidth = 900,
            MinimumHeight = 620
        };
    }
}
