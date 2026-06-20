# ViewModels

This folder contains **ViewModels** for the application.

## Global ViewModels

Place application-wide ViewModels here (e.g., `AppShellViewModel.cs`).

## Feature-Specific ViewModels

Feature ViewModels belong in their respective feature folders:
- `Features/[Feature]/ViewModels/[Page]ViewModel.cs`

## Structure

```
ViewModels/
├── Base/
│   └── BaseViewModel.cs    # Abstract base for all ViewModels
├── AppShellViewModel.cs    # Application shell state
└── [Feature ViewModels]    # In Features/[Feature]/ViewModels/
```

## Best Practices

✅ Inherit from `BaseViewModel`
✅ Use `[RelayCommand]` for user actions
✅ Use `[ObservableProperty]` for reactive properties
✅ Inject dependencies via constructor
✅ No static UI references (`Shell.Current`, etc.)
✅ Keep testable with dependency injection
✅ Implement navigation lifecycle (`OnNavigatedTo`, `OnNavigatedFrom`)
✅ Add XML documentation

## Example ViewModel

```csharp
using HCWMauiApp.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

public partial class MyPageViewModel : BaseViewModel
{
    private readonly IMyService _service;

    [ObservableProperty]
    private string userName = string.Empty;

    public MyPageViewModel(IMyService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Title = "My Page";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            var data = await _service.GetDataAsync();
            UserName = data.Name;
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
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
            await LoadAsync();
            IsInitialized = true;
        }
    }
}
```

See [MAUI-ARCHITECTURE.md](../../docs/application/MAUI-ARCHITECTURE.md) for detailed patterns.
