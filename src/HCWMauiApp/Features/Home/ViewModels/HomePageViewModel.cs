using HCWMauiApp.ViewModels.Base;

namespace HCWMauiApp.Features.Home.ViewModels;

/// <summary>
/// ViewModel for the Home feature's main page.
/// Example: Remove this file and create your own feature ViewModels.
/// </summary>
public partial class HomePageViewModel : BaseViewModel
{
    public HomePageViewModel()
    {
        Title = "Home";
    }

    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            // TODO: Implement your data loading logic here
            await Task.Delay(500); // Simulate API call

            // Update UI with data
        }
        catch (Exception ex)
        {
            SetError($"Failed to load data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public override async Task OnNavigatedTo()
    {
        if (!IsInitialized)
        {
            await LoadDataAsync();
            IsInitialized = true;
        }
    }
}
