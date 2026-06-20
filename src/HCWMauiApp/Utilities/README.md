# Utilities

This folder contains **helper functions**, **extension methods**, and **constants**.

## Structure

```
Utilities/
├── Constants/
│   ├── AppConstants.cs       # App-wide constants
│   └── ApiConstants.cs       # API endpoints, timeouts
├── Extensions/
│   ├── StringExtensions.cs
│   ├── CollectionExtensions.cs
│   └── TaskExtensions.cs
└── Helpers/
    ├── DateTimeHelper.cs
    ├── ValidationHelper.cs
    └── PlatformHelper.cs
```

## Best Practices

✅ Keep helpers small and focused
✅ Use extension methods for common operations
✅ Add XML documentation
✅ Keep functions pure and stateless
✅ Use static classes for utilities

## Extension Methods Example

```csharp
namespace HCWMauiApp.Utilities;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    public static string ToTitleCase(this string value)
        => System.Globalization.CultureInfo.CurrentCulture
            .TextInfo.ToTitleCase(value.ToLower());
}
```

## Constants Example

```csharp
namespace HCWMauiApp.Utilities.Constants;

public static class ApiConstants
{
    public const string BaseUrl = "https://api.example.com";
    public const int RequestTimeoutMs = 30000;
    public const int MaxRetries = 3;

    public static class Endpoints
    {
        public const string Users = "/users";
        public const string Auth = "/auth";
        public const string Products = "/products";
    }
}
```

## Helper Methods Example

```csharp
namespace HCWMauiApp.Utilities.Helpers;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) &&
           email.Contains("@") &&
           email.Contains(".");

    public static bool IsValidPhoneNumber(string phone)
        => !string.IsNullOrWhiteSpace(phone) &&
           phone.Length >= 10;
}
```

## Usage

```csharp
using HCWMauiApp.Utilities;
using HCWMauiApp.Utilities.Constants;
using HCWMauiApp.Utilities.Helpers;

// Extension methods
if (!userName.IsNullOrEmpty())
{
    label.Text = userName.ToTitleCase();
}

// Constants
var client = new HttpClient();
client.BaseAddress = new Uri(ApiConstants.BaseUrl);
client.Timeout = TimeSpan.FromMilliseconds(ApiConstants.RequestTimeoutMs);

// Helpers
if (ValidationHelper.IsValidEmail(email))
{
    // Proceed
}
```
