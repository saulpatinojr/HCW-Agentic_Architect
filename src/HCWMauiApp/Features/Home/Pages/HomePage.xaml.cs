namespace HCWMauiApp.Features.Home.Pages;

using HCWMauiApp.Features.Home.ViewModels;

public partial class HomePage : ContentPage
{
    public HomePage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
