# Services

This folder contains **business logic** and **cross-cutting concerns**.

## Structure

```
Services/
├── Interfaces/         # Service contracts
│   ├── IApiClient.cs
│   ├── IAuthService.cs
│   ├── IStorageService.cs
│   └── IUserService.cs
├── Http/
│   ├── ApiClient.cs
│   └── AuthHandler.cs
├── Storage/
│   ├── LocalStorageService.cs
│   └── SecureStorageService.cs
├── Auth/
│   └── AuthService.cs
└── Navigation/
    └── NavigationService.cs
```

## Best Practices

✅ **Always create interfaces first** - enables testability
✅ Inject via constructor
✅ Single Responsibility Principle
✅ Async/await for I/O operations
✅ Proper error handling and logging
✅ Register in `MauiProgram.cs`
✅ Add XML documentation

## Service Template

```csharp
using HCWMauiApp.Services.Interfaces;

namespace HCWMauiApp.Services;

public class MyService : IMyService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MyService> _logger;

    public MyService(IApiClient apiClient, ILogger<MyService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T> GetDataAsync<T>()
    {
        try
        {
            _logger.LogInformation("Fetching data...");
            return await _apiClient.GetAsync<T>("/api/data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data");
            throw;
        }
    }
}
```

## Service Lifetimes

| Lifetime | Usage | Example |
|----------|-------|---------|
| **Singleton** | App-wide single instance | Logger, Configuration, Navigation |
| **Scoped** | Per-request instance | (rare in MAUI) |
| **Transient** | New instance each time | ViewModels, Commands |

**Registration:**
```csharp
// Singleton - same instance app-wide
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// Transient - new instance each time
builder.Services.AddTransient<HomePage>();
```

## Common Services

- **IApiClient** - HTTP communication
- **IAuthService** - Authentication & authorization
- **IStorageService** - Local/secure data storage
- **INavigationService** - App navigation
- **ILogger** - Logging (built-in)
- **IUserService** - User domain logic

See [MAUI-ARCHITECTURE.md](../../docs/application/MAUI-ARCHITECTURE.md) for detailed examples.
