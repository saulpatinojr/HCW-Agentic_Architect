# MAUI Development Quick Reference

## Starting a New Feature

### Step 1: Create Folder Structure
```
Features/YourFeature/
├── Pages/
│   ├── YourFeaturePage.xaml
│   └── YourFeaturePage.xaml.cs
└── ViewModels/
    └── YourFeaturePageViewModel.cs
```

### Step 2: Create ViewModel
```csharp
using HCWMauiApp.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

public partial class YourFeaturePageViewModel : BaseViewModel
{
    private readonly IYourService _service;

    [ObservableProperty]
    private string yourData = string.Empty;

    public YourFeaturePageViewModel(IYourService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Title = "Your Feature";
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();
            YourData = await _service.GetDataAsync();
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
            await LoadDataAsync();
            IsInitialized = true;
        }
    }
}
```

### Step 3: Create XAML Page
```xml
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="HCWMauiApp.Features.YourFeature.Pages.YourFeaturePage"
    Title="{Binding Title}">

    <VerticalStackLayout Padding="20" Spacing="10">
        <Label Text="{Binding YourData}" FontSize="18" />
        
        <Button 
            Text="Load Data"
            Command="{Binding LoadDataCommand}"
            IsEnabled="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}" />
        
        <Label 
            Text="{Binding ErrorMessage}"
            IsVisible="{Binding ErrorMessage, StringFormat='{0}', Converter={StaticResource StringNullOrEmptyToBoolConverter}, ConverterParameter=True}"
            TextColor="Red" />
    </VerticalStackLayout>
</ContentPage>
```

### Step 4: Create Code-Behind
```csharp
namespace HCWMauiApp.Features.YourFeature.Pages;
using HCWMauiApp.Features.YourFeature.ViewModels;

public partial class YourFeaturePage : ContentPage
{
    public YourFeaturePage(YourFeaturePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### Step 5: Register in MauiProgram.cs
```csharp
builder.Services
    .AddTransient<YourFeaturePage>()
    .AddTransient<YourFeaturePageViewModel>();
```

### Step 6: Add Route to AppShell.xaml
```xml
<ShellContent 
    Title="Your Feature" 
    ContentTemplate="{DataTemplate local:YourFeaturePage}"
    Route="yourfeature" />
```

---

## Creating a Service

### Step 1: Create Interface
```csharp
// Services/Interfaces/IYourService.cs
namespace HCWMauiApp.Services.Interfaces;

public interface IYourService
{
    Task<string> GetDataAsync();
    Task SaveDataAsync(string data);
}
```

### Step 2: Create Implementation
```csharp
// Services/YourService.cs
namespace HCWMauiApp.Services;

public class YourService : IYourService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<YourService> _logger;

    public YourService(IApiClient apiClient, ILogger<YourService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetDataAsync()
    {
        try
        {
            _logger.LogInformation("Fetching data...");
            return await _apiClient.GetAsync<string>("/api/data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data");
            throw;
        }
    }

    public async Task SaveDataAsync(string data)
    {
        try
        {
            _logger.LogInformation("Saving data...");
            await _apiClient.PostAsync("/api/data", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data");
            throw;
        }
    }
}
```

### Step 3: Register in MauiProgram.cs
```csharp
builder.Services.AddSingleton<IYourService, YourService>();
```

---

## Common Code Patterns

### Async Command with Loading State
```csharp
[RelayCommand]
public async Task DoSomethingAsync()
{
    try
    {
        IsLoading = true;
        ClearError();
        
        // Do work
        await Task.Delay(1000);
        
    }
    catch (Exception ex)
    {
        SetError($"Failed: {ex.Message}");
    }
    finally
    {
        IsLoading = false;
    }
}
```

### List Binding with Clear/AddRange
```csharp
[ObservableProperty]
private ObservableCollection<Item> items = new();

public async Task LoadItemsAsync()
{
    try
    {
        IsLoading = true;
        var newItems = await _service.GetItemsAsync();
        Items.Clear();
        Items.AddRange(newItems);
    }
    finally
    {
        IsLoading = false;
    }
}
```

### Selected Item from List
```csharp
[ObservableProperty]
private Item? selectedItem;

partial void OnSelectedItemChanged(Item? value)
{
    if (value != null)
    {
        // Handle selection
        Shell.Current.GoToAsync($"details?id={value.Id}");
    }
}
```

### Service Injection Pattern
```csharp
public MyViewModel(
    IFirstService firstService,
    ISecondService secondService,
    ILogger<MyViewModel> logger)
{
    _firstService = firstService ?? throw new ArgumentNullException(nameof(firstService));
    _secondService = secondService ?? throw new ArgumentNullException(nameof(secondService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

---

## XAML Binding Patterns

### Two-Way Binding
```xml
<Entry Text="{Binding UserName, Mode=TwoWay}" />
```

### Command Binding
```xml
<Button Text="Save" Command="{Binding SaveCommand}" />
```

### Command with Parameter
```xml
<Button Text="Delete" Command="{Binding DeleteCommand}" CommandParameter="{Binding Item}" />
```

### Converter Binding
```xml
<BoxView Color="{Binding IsActive, Converter={StaticResource BoolToColorConverter}}" />
```

### String Formatting
```xml
<Label Text="{Binding User.Name, StringFormat='Hello, {0}!'}" />
```

### Binding with Fallback
```xml
<Label Text="{Binding ErrorMessage, FallbackValue='No errors'}" />
```

---

## Testing Patterns

### Test Naming Convention
```
{Method}_{Scenario}_{ExpectedResult}

Examples:
- LoadData_WithValidInput_ReturnsData
- SaveUser_WithInvalidData_ThrowsException
- GetById_WithNullId_ReturnsNull
```

### Basic Unit Test
```csharp
[Fact]
public async Task LoadDataAsync_WithValidData_SetsLoadingFalse()
{
    // Arrange
    var mockService = new Mock<IMyService>();
    mockService.Setup(s => s.GetDataAsync())
        .ReturnsAsync("test data");
    
    var viewModel = new MyPageViewModel(mockService.Object);

    // Act
    await viewModel.LoadDataAsync();

    // Assert
    Assert.False(viewModel.IsLoading);
    Assert.Equal("test data", viewModel.Data);
    mockService.Verify(s => s.GetDataAsync(), Times.Once);
}
```

### Mock Setup Examples
```csharp
// Return value
mockService.Setup(s => s.GetData()).Returns(expectedValue);

// Return async value
mockService.Setup(s => s.GetDataAsync()).ReturnsAsync(expectedValue);

// Throw exception
mockService.Setup(s => s.GetData()).Throws<ArgumentException>();

// Verify called
mockService.Verify(s => s.GetData(), Times.Once);

// Verify called with specific argument
mockService.Verify(s => s.GetData(It.Is<int>(id => id == 5)), Times.Once);
```

---

## Directory Reference

| Folder | When to Use | Example |
|--------|-----------|---------|
| **Features/** | Business domain features | `Features/Users/`, `Features/Settings/` |
| **Services/** | Business logic & APIs | User service, API client |
| **Models/** | Data entities | `User.cs`, `UserDTO.cs` |
| **ViewModels/** | Presentation logic | `HomePageViewModel.cs` |
| **Resources/** | Styles & assets | `AppResources.xaml`, images |
| **Converters/** | Value conversion | `BoolToColorConverter.cs` |
| **Behaviors/** | UI behaviors | `NumericValidationBehavior.cs` |
| **Utilities/** | Helpers & extensions | `StringExtensions.cs` |
| **Platforms/** | Platform-specific code | iOS, Android setup |

---

## File Naming Quick Ref

```
XAML Pages:          [Feature]Page.xaml
Code-Behind:         [Feature]Page.xaml.cs
ViewModels:          [Feature]PageViewModel.cs
Services:            [Feature]Service.cs
Interfaces:          I[Feature]Service.cs
Models:              [Entity].cs
Converters:          [From]To[To]Converter.cs
Behaviors:           [Behavior]Behavior.cs
```

---

## Essential Documentation

- **src/HCWMauiApp/README.md** - Overview & structure
- **wiki/MAUI-ARCHITECTURE.md** - Full architecture guide
- **src/README.md** - Directory explanations
- **[Folder]/README.md** - Specific guidance for each directory

See the appropriate README.md for detailed information on each area.
