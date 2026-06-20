# HCW Agentic Architect - Application

Complete .NET MAUI application skeleton and best practices guide for cross-platform development.

## 📁 Structure Overview

```
src/
├── HCWMauiApp/                    # Main MAUI application
│   ├── Features/                  # Feature-based organization
│   │   ├── Home/
│   │   │   ├── Pages/
│   │   │   └── ViewModels/
│   │   └── Settings/
│   │       ├── Pages/
│   │       └── ViewModels/
│   ├── ViewModels/                # App-wide ViewModels
│   │   ├── Base/
│   │   │   └── BaseViewModel.cs
│   │   └── AppShellViewModel.cs
│   ├── Services/                  # Business logic & APIs
│   │   ├── Interfaces/
│   │   ├── Http/
│   │   ├── Storage/
│   │   ├── Auth/
│   │   └── Navigation/
│   ├── Models/                    # Domain entities & DTOs
│   ├── Resources/                 # Images, fonts, styles
│   ├── Converters/                # XAML value converters
│   ├── Behaviors/                 # Reusable UI behaviors
│   ├── Utilities/                 # Helpers & extensions
│   ├── Platforms/                 # Platform-specific code
│   ├── App.xaml(.cs)              # Application root
│   ├── AppShell.xaml(.cs)         # Navigation shell
│   └── MauiProgram.cs             # DI configuration
│
└── tests/                         # Unit & integration tests
    ├── HCWMauiApp.Tests/
    └── HCWMauiApp.Integration/
```

## 🎯 Quick Start

### 1. **Understand the Architecture**
Read [docs/application/MAUI-ARCHITECTURE.md](../docs/application/MAUI-ARCHITECTURE.md)

### 2. **Create a New Feature**
```
Features/YourFeature/
├── Pages/
│   ├── YourFeaturePage.xaml
│   └── YourFeaturePage.xaml.cs
└── ViewModels/
    └── YourFeaturePageViewModel.cs
```

### 3. **Create a Service**
```csharp
// Services/Interfaces/IYourService.cs
public interface IYourService
{
    Task<T> GetDataAsync();
}

// Services/YourService.cs
public class YourService : IYourService { }
```

### 4. **Register in MauiProgram.cs**
```csharp
builder.Services
    .AddSingleton<IYourService, YourService>()
    .AddTransient<YourFeaturePageViewModel>();
```

## 📚 Directory Guide

| Directory | Purpose | See Also |
|-----------|---------|----------|
| `Features/` | Feature modules by business domain | [Features/README.md](HCWMauiApp/Features/README.md) |
| `ViewModels/` | MVVM presentation logic | [ViewModels/README.md](HCWMauiApp/ViewModels/README.md) |
| `Services/` | Business logic & integrations | [Services/README.md](HCWMauiApp/Services/README.md) |
| `Models/` | Domain entities & DTOs | [Models/README.md](HCWMauiApp/Models/README.md) |
| `Resources/` | App-wide resources | [Resources/README.md](HCWMauiApp/Resources/README.md) |
| `Converters/` | XAML value converters | [Converters/README.md](HCWMauiApp/Converters/README.md) |
| `Behaviors/` | Reusable XAML behaviors | [Behaviors/README.md](HCWMauiApp/Behaviors/README.md) |
| `Utilities/` | Helpers & extensions | [Utilities/README.md](HCWMauiApp/Utilities/README.md) |
| `Platforms/` | Platform-specific code | [Platforms/README.md](HCWMauiApp/Platforms/README.md) |
| `tests/` | Unit & integration tests | [tests/README.md](../tests/README.md) |

## 🏗️ Architecture Pattern

**MVVM with Clean Architecture**

```
User Interaction (XAML)
        ↓ (Binding)
ViewModel (Handles state & commands)
        ↓ (Calls)
Services (Business logic)
        ↓ (Works with)
Models (Data structures)
        ↓ (Persists via)
API / Local Storage
```

## ✅ Best Practices Implemented

- ✅ **Feature-based organization** - Code organized by business domain
- ✅ **MVVM pattern** - Clear separation of UI logic and business logic
- ✅ **Dependency injection** - All services registered in DI container
- ✅ **Interface-first design** - All services have interfaces for testability
- ✅ **Async/await throughout** - No blocking operations
- ✅ **Proper error handling** - Try-catch with logging
- ✅ **Centralized resources** - Consistent theming and styling
- ✅ **Platform abstraction** - Platform-specific code isolated
- ✅ **Test structure** - Organized for unit and integration tests
- ✅ **Documentation** - Every folder has guidance README

## 🔍 Key Files

| File | Purpose |
|------|---------|
| `App.xaml` | Application root, app-wide resources |
| `AppShell.xaml` | Navigation shell and routing |
| `MauiProgram.cs` | Dependency injection configuration |
| `ViewModels/Base/BaseViewModel.cs` | Base class for all ViewModels |

## 📖 Example ViewModel

```csharp
using HCWMauiApp.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

public partial class HomePageViewModel : BaseViewModel
{
    private readonly IExampleService _service;

    [ObservableProperty]
    private string greeting = string.Empty;

    public HomePageViewModel(IExampleService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Title = "Home";
    }

    [RelayCommand]
    public async Task LoadGreetingAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            var data = await _service.GetDataAsync();
            Greeting = $"Welcome!";
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
            await LoadGreetingAsync();
            IsInitialized = true;
        }
    }
}
```

## 📋 Example Service

```csharp
using HCWMauiApp.Services.Interfaces;

public class ExampleService : IExampleService
{
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(ILogger<ExampleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<object> GetDataAsync()
    {
        try
        {
            _logger.LogInformation("Fetching data...");
            // TODO: Implement
            return new object();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
            throw;
        }
    }
}
```

## 🧪 Testing

Tests mirror the source structure:

```
tests/HCWMauiApp.Tests/
├── ViewModels/
│   └── HomePageViewModelTests.cs
├── Services/
│   └── ExampleServiceTests.cs
└── Converters/
    └── BoolToColorConverterTests.cs
```

Run tests:
```bash
dotnet test
```

## 🎨 Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| Files | PascalCase | `HomePage.xaml`, `HomePageViewModel.cs` |
| Classes | PascalCase | `HomePageViewModel`, `UserService` |
| Properties | PascalCase | `IsLoading`, `UserName` |
| Private fields | _camelCase | `_apiClient`, `_logger` |
| Constants | UPPER_CASE | `API_BASE_URL`, `MAX_RETRIES` |
| Interfaces | IPascalCase | `IUserService`, `IApiClient` |

## 🚀 Next Steps

1. **Create project files** (.csproj, App.xaml, AppShell.xaml, MauiProgram.cs)
2. **Implement base classes** (BaseViewModel, BaseService)
3. **Build your first feature** using the examples
4. **Add services** for API calls and data access
5. **Create unit tests** for ViewModels and Services
6. **Deploy** to your target platforms

## 📖 Resources

- [MAUI Architecture Guide](../docs/application/MAUI-ARCHITECTURE.md)
- [Official MAUI Docs](https://learn.microsoft.com/dotnet/maui)
- [MVVM Toolkit](https://learn.microsoft.com/windows/communitytoolkit/mvvm/mvvm_introduction)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)

---

**Questions?** Check the README.md files in each directory for detailed guidance.
