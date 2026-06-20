# MAUI Application Architecture

## Overview

The HCW Workspace Manager MAUI application follows a **clean architecture** pattern with **MVVM** (Model-View-ViewModel) for clear separation of concerns, testability, and maintainability.

## Core Architectural Principles

### 1. **Separation of Concerns**
- **UI Layer**: XAML pages and markup
- **Presentation Layer**: ViewModels handle user interaction and state
- **Business Layer**: Services contain business logic
- **Data Layer**: Models represent domain entities

### 2. **Dependency Injection**
All services are registered in `MauiProgram.cs` and injected via constructors. This enables:
- Easy testing (mock dependencies)
- Loose coupling
- Runtime configuration

### 3. **MVVM Pattern**

```
View (XAML Page)
      ↓ (BindingContext)
ViewModel (Handles user interaction & state)
      ↓ (Depends on)
Services (Business logic, API calls, data access)
      ↓ (Work with)
Models (Data structures)
```

## Key Components

### BaseViewModel
Located: `ViewModels/Base/BaseViewModel.cs`

All ViewModels inherit from `BaseViewModel` which provides:
- Property change notifications (`INotifyPropertyChanged`)
- Loading state management
- Error handling
- Navigation lifecycle methods

```csharp
public class MyPageViewModel : BaseViewModel
{
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Fetch data
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
}
```

### Services & Interfaces

**Always create interfaces first**:

```csharp
// IUserService.cs
public interface IUserService
{
    Task<User> GetUserAsync(int id);
    Task SaveUserAsync(User user);
}

// UserService.cs
public class UserService : IUserService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<UserService> _logger;

    public UserService(IApiClient apiClient, ILogger<UserService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<User> GetUserAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching user {UserId}", id);
            return await _apiClient.GetAsync<User>($"/users/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user");
            throw;
        }
    }

    public async Task SaveUserAsync(User user)
    {
        await _apiClient.PostAsync("/users", user);
    }
}
```

Register in `MauiProgram.cs`:
```csharp
builder.Services
    .AddSingleton<IApiClient, ApiClient>()
    .AddSingleton<IUserService, UserService>();
```

### Feature Organization

Each feature (Home, Settings, Users, etc.) is self-contained:

```
Features/Users/
├── Pages/
│   ├── UsersPage.xaml
│   ├── UsersPage.xaml.cs
│   ├── UserDetailPage.xaml
│   └── UserDetailPage.xaml.cs
├── ViewModels/
│   ├── UsersPageViewModel.cs
│   └── UserDetailPageViewModel.cs
└── Models/ (optional, feature-specific models)
    └── UserFilterModel.cs
```

**XAML Code-Behind** (should be minimal):
```csharp
public partial class UsersPage : ContentPage
{
    public UsersPage(UsersPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**ViewModel**:
```csharp
public partial class UsersPageViewModel : BaseViewModel
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private ObservableCollection<User> users = new();

    [ObservableProperty]
    private User? selectedUser;

    public UsersPageViewModel(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        Title = "Users";
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var users = await _userService.GetUsersAsync();
            Users.Clear();
            Users.AddRange(users);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SelectUserAsync(User user)
    {
        SelectedUser = user;
        // Navigate to detail page
        await Shell.Current.GoToAsync($"userdetail?id={user.Id}");
    }

    public override async Task OnNavigatedTo()
    {
        if (!IsInitialized)
        {
            await LoadUsersAsync();
            IsInitialized = true;
        }
    }
}
```

### Data Models

Keep models simple:

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

// For API responses (DTOs)
public record UserResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt
);
```

### Resources & Styling

Centralize theme colors and styles:

**AppResources.xaml**:
```xml
<ResourceDictionary>
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#512BD4</Color>
    <Color x:Key="SecondaryColor">#DFD8F7</Color>
    <Color x:Key="TextColor">#212121</Color>
    
    <!-- Sizes -->
    <x:Double x:Key="FontSizeTitle">24</x:Double>
    <x:Double x:Key="FontSizeBody">14</x:Double>
    
    <!-- Styles -->
    <Style TargetType="Label" x:Key="TitleLabel">
        <Setter Property="FontSize" Value="{StaticResource FontSizeTitle}" />
        <Setter Property="TextColor" Value="{StaticResource TextColor}" />
    </Style>
</ResourceDictionary>
```

### Error Handling

**Pattern**: Catch → Log → Handle → User Feedback

```csharp
try
{
    // Operation
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API call failed");
    SetError("Network error. Please check your connection.");
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Invalid argument");
    SetError("Invalid input provided.");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    SetError("An unexpected error occurred. Please try again.");
}
finally
{
    IsLoading = false;
}
```

### Async/Await Best Practices

✅ **Good**:
```csharp
public async Task LoadDataAsync()
{
    var data = await _service.GetDataAsync();
    ProcessData(data);
}
```

❌ **Avoid**:
```csharp
public void LoadData()
{
    var task = _service.GetDataAsync();
    task.Wait(); // Blocks UI thread
}
```

## Application Configuration

### MauiProgram.cs Structure

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => /* ... */)
            
            // Services
            .AddApplicationServices()
            .AddHttpClients()
            .AddStorageServices()
            
            // Pages & ViewModels
            .AddPages()
            .AddViewModels()
            
            // Logging
            .ConfigureLogging();

        return builder.Build();
    }

    private static MauiAppBuilder AddApplicationServices(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<IApiClient, ApiClient>()
            .AddSingleton<IUserService, UserService>();
        return builder;
    }

    private static MauiAppBuilder AddPages(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<HomePage>()
            .AddSingleton<UsersPage>();
        return builder;
    }

    private static MauiAppBuilder AddViewModels(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<HomePageViewModel>()
            .AddSingleton<UsersPageViewModel>();
        return builder;
    }
}
```

## Testing Strategy

### Unit Tests
- Test ViewModels in isolation
- Mock services and dependencies
- Verify commands, property changes, error handling

```csharp
[Fact]
public async Task LoadUsersAsync_WithValidData_PopulatesUsers()
{
    // Arrange
    var mockService = new Mock<IUserService>();
    var users = new[] { new User { Id = 1, Name = "John" } };
    mockService.Setup(s => s.GetUsersAsync())
        .ReturnsAsync(users);
    
    var viewModel = new UsersPageViewModel(mockService.Object);

    // Act
    await viewModel.LoadUsersAsync();

    // Assert
    Assert.Single(viewModel.Users);
    Assert.Equal("John", viewModel.Users[0].Name);
}
```

### Integration Tests
- Test services with real dependencies
- Test API integration
- Test data persistence

## File Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| Pages (XAML) | `[Feature]Page.xaml` | `HomePage.xaml` |
| Code-Behind | `[Feature]Page.xaml.cs` | `HomePage.xaml.cs` |
| ViewModels | `[Feature]PageViewModel.cs` | `HomePageViewModel.cs` |
| Services | `[Domain]Service.cs` | `UserService.cs` |
| Interfaces | `I[Service]Service.cs` | `IUserService.cs` |
| Models | `[Entity].cs` | `User.cs` |
| Converters | `[From]To[To]Converter.cs` | `BoolToColorConverter.cs` |

## Common Pitfalls to Avoid

❌ **Putting business logic in code-behind**
✅ Move it to ViewModel → Service

❌ **Static UI access** (`Shell.Current`, `App.Current`)
✅ Inject INavigationService instead

❌ **Long-running operations on UI thread**
✅ Always use async/await

❌ **Not handling exceptions**
✅ Always wrap in try-catch and log

❌ **Tight coupling** (creating services directly)
✅ Use dependency injection

❌ **Not using interfaces**
✅ All services should have interfaces for testability

## Performance Tips

1. **Virtual lists** for large data sets (CollectionView with ItemsSource)
2. **Lazy loading** for list items
3. **Async all the way** - no blocking calls
4. **Resource cleanup** - dispose of services properly
5. **Image optimization** - use appropriate sizes and formats
6. **Navigation efficiency** - avoid excessive navigation events

## Next Steps

1. Create your feature folders under `Features/`
2. Create ViewModels inheriting from `BaseViewModel`
3. Create Services with corresponding interfaces
4. Register services in `MauiProgram.cs`
5. Create XAML pages and bind to ViewModels
6. Add unit tests for ViewModels and Services

---

**See also**: 
- [src/README.md](../../src/README.md) - Directory structure details
- [MAUI Docs](https://learn.microsoft.com/dotnet/maui)
- [MVVM Toolkit](https://learn.microsoft.com/windows/communitytoolkit/mvvm/mvvm_introduction)
