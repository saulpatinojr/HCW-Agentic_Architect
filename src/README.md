# HCW Agentic Architect - MAUI Application

A .NET MAUI application following clean architecture and best practices for cross-platform mobile and desktop development.

## Project Structure

```
src/
└── HCWMauiApp/                    # Main MAUI application
    ├── Features/                  # Feature-based organization (MVVM)
    ├── Models/                    # Data models and entities
    ├── ViewModels/                # Application-level ViewModels
    ├── Services/                  # Business logic and integrations
    ├── Resources/                 # Images, fonts, styles
    ├── Converters/                # Value converters for binding
    ├── Behaviors/                 # Custom XAML behaviors
    ├── Utilities/                 # Helper functions and extensions
    ├── Platforms/                 # Platform-specific code
    ├── App.xaml(.cs)              # Application entry point
    ├── AppShell.xaml(.cs)         # Shell navigation structure
    ├── MauiProgram.cs             # DI and service configuration
    └── [Project files]            # .csproj, .xaml, etc.

tests/                            # Unit and integration tests
```

## Directory Guide

### Features/
**Purpose**: Organize pages, ViewModels, and feature-specific logic by business domain.

```
Features/
├── Home/
│   ├── Pages/                  # XAML pages for this feature
│   │   └── HomePage.xaml(.cs)
│   ├── ViewModels/             # Feature-specific ViewModels
│   │   └── HomePageViewModel.cs
│   └── Services/               # Feature-specific services (optional)
└── Settings/
    ├── Pages/
    │   └── SettingsPage.xaml(.cs)
    └── ViewModels/
        └── SettingsPageViewModel.cs
```

**When to create a new feature**:
- When a feature has multiple related pages/screens
- When logic is cohesive and can be reused within that feature
- Each feature is independently testable

**Best Practices**:
- Keep features modular and self-contained
- Use feature-specific ViewModels (inherit from base ViewModel)
- Navigation routes should reference feature names

### Models/
**Purpose**: Domain entities, DTOs, and data contracts.

```
Models/
├── Domain/                     # Core business models
│   └── User.cs
├── DTOs/                       # Data Transfer Objects
│   └── UserDTO.cs
└── Responses/                  # API response models
    └── ApiResponse.cs
```

**Best Practices**:
- Keep models simple (properties, no logic)
- Use nullable reference types
- Implement INotifyPropertyChanged only in ViewModels, not Models
- Use records for immutable DTOs

### ViewModels/
**Purpose**: Presentation logic and state management (MVVM pattern).

```
ViewModels/
├── Base/
│   ├── BaseViewModel.cs        # Common ViewModel functionality
│   └── ObservableObject.cs
├── AppShellViewModel.cs        # Shell-level state
└── [Feature ViewModels]        # Features/[Feature]/ViewModels/
```

**Best Practices**:
- Inherit from `BaseViewModel`
- Implement `INotifyPropertyChanged`
- Use Commands for user actions
- Keep ViewModels testable (inject dependencies)
- No direct UI references (no `Shell.Current`, etc.)

### Services/
**Purpose**: Business logic, API calls, data access, and cross-cutting concerns.

```
Services/
├── Interfaces/                 # Service contracts
│   ├── IAuthService.cs
│   ├── IApiClient.cs
│   └── IStorageService.cs
├── Http/
│   ├── ApiClient.cs            # HTTP communication
│   └── AuthHandler.cs          # Auth middleware
├── Storage/
│   ├── LocalStorageService.cs
│   └── SecureStorageService.cs
├── Auth/
│   └── AuthService.cs
└── Navigation/
    └── NavigationService.cs
```

**Best Practices**:
- Always define interfaces (`IAuthService`)
- Inject via constructor
- Use dependency injection (DI) container
- Services should have single responsibility
- Async/await for I/O operations

### Resources/
**Purpose**: App-wide resources (images, fonts, colors, styles).

```
Resources/
├── AppResources.xaml           # Colors, sizes, strings
├── Styles/
│   ├── Colors.xaml
│   ├── Fonts.xaml
│   └── Styles.xaml
├── Images/
│   ├── icon.png
│   └── splash.png
└── Fonts/
    └── custom-font.ttf
```

**Best Practices**:
- Centralize theme colors and dimensions
- Use consistent naming (e.g., `PrimaryColor`, `FontSizeTitle`)
- Store app strings in resource files for localization
- Images organized by size/type (icons, backgrounds, etc.)

### Converters/
**Purpose**: Value converters for XAML data binding.

```
Converters/
├── BoolToColorConverter.cs
├── InverseBoolConverter.cs
└── DateTimeToStringConverter.cs
```

**Best Practices**:
- One converter per file
- Implement `IValueConverter`
- Keep conversion logic simple
- Handle null values gracefully

### Behaviors/
**Purpose**: Encapsulate reusable attached behavior logic.

```
Behaviors/
├── NumericValidationBehavior.cs
└── EventToCommandBehavior.cs
```

**Best Practices**:
- Inherit from `Behavior<T>`
- Attach to UI elements in XAML
- Use for validation, animation triggers, etc.

### Utilities/
**Purpose**: Helper functions, extension methods, constants.

```
Utilities/
├── Constants/
│   ├── AppConstants.cs
│   └── ApiConstants.cs
├── Extensions/
│   ├── StringExtensions.cs
│   └── CollectionExtensions.cs
└── Helpers/
    ├── DateTimeHelper.cs
    └── ValidationHelper.cs
```

**Best Practices**:
- Organize by category (Constants, Extensions, Helpers)
- Keep functions pure and stateless
- Use extension methods for common operations
- Add XML documentation

### Platforms/
**Purpose**: Platform-specific code (iOS, Android, Windows, macOS).

```
Platforms/
├── iOS/
│   └── Info.plist
├── Android/
│   ├── AndroidManifest.xml
│   └── MainActivity.cs
├── Windows/
│   └── App.xaml
└── MacCatalyst/
    └── Info.plist
```

**Best Practices**:
- Use `#if` directives sparingly
- Create platform-specific services with common interfaces
- Use MAUI's platform-specific properties when possible

### Tests/
**Purpose**: Unit and integration tests.

```
tests/
├── HCWMauiApp.Tests/
│   ├── ViewModels/
│   ├── Services/
│   ├── Converters/
│   └── Utilities/
└── HCWMauiApp.Integration/
    ├── Api/
    └── Storage/
```

**Best Practices**:
- Mirror source structure in tests
- One test class per class being tested
- Use descriptive test method names (e.g., `LoadUsers_WithValidInput_ReturnsUserList`)
- Mock external dependencies

## Architecture Pattern

This project uses **MVVM (Model-View-ViewModel)** with a focus on:

- **Separation of Concerns**: UI logic in ViewModels, business logic in Services
- **Testability**: Dependency injection and interfaces
- **Maintainability**: Clear folder structure and naming conventions
- **Scalability**: Feature-based organization allows easy addition of new features

## Getting Started

1. **Create a Feature**:
   ```
   Features/
   └── MyFeature/
       ├── Pages/
       │   └── MyFeaturePage.xaml(.cs)
       ├── ViewModels/
       │   └── MyFeaturePageViewModel.cs
       └── Services/ (optional)
   ```

2. **Create a ViewModel**:
   - Inherit from `BaseViewModel`
   - Use `OnPropertyChanged()` for reactive updates
   - Implement Commands for user actions

3. **Create a Service**:
   - Define interface in `Services/Interfaces/`
   - Implement in appropriate subfolder
   - Register in `MauiProgram.cs`

4. **Register in DI Container** (`MauiProgram.cs`):
   ```csharp
   builder.Services
       .AddSingleton<IMyService, MyService>()
       .AddTransient<MyPageViewModel>();
   ```

## Naming Conventions

- **Files**: PascalCase (e.g., `HomePage.xaml`, `HomePageViewModel.cs`)
- **Classes**: PascalCase (e.g., `HomePageViewModel`, `ApiClient`)
- **Properties**: PascalCase (e.g., `IsLoading`, `UserName`)
- **Fields**: _camelCase (e.g., `_apiClient`)
- **Constants**: UPPER_CASE (e.g., `API_BASE_URL`)
- **XAML Resources**: camelCase or descriptive (e.g., `primaryColor`, `title2Font`)

## Key Files

| File | Purpose |
|------|---------|
| `App.xaml(.cs)` | Application root, theme definitions |
| `AppShell.xaml(.cs)` | Navigation shell and routing |
| `MauiProgram.cs` | DI configuration, service registration |

## Best Practices Checklist

- ✅ Use async/await for I/O operations
- ✅ Implement proper error handling and logging
- ✅ Use dependency injection for all services
- ✅ Keep ViewModels testable (no static references)
- ✅ Use interfaces for all services
- ✅ Organize features by business domain
- ✅ Avoid UI code in ViewModels
- ✅ Use commands for user interactions
- ✅ Implement property change notifications
- ✅ Keep models simple data structures

## Useful Resources

- [MAUI Documentation](https://learn.microsoft.com/dotnet/maui)
- [MVVM Toolkit](https://learn.microsoft.com/windows/communitytoolkit/mvvm/mvvm_introduction)
- [Prism for MAUI](https://github.com/PrismLibrary/Prism)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
