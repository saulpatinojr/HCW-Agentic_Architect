# Platforms

This folder contains **platform-specific code** for iOS, Android, Windows, and macOS.

## Structure

```
Platforms/
├── iOS/
│   └── Info.plist             # iOS configuration
├── Android/
│   ├── AndroidManifest.xml    # Android configuration
│   └── MainActivity.cs        # Android entry point
├── Windows/
│   └── App.xaml               # Windows app config
└── MacCatalyst/
    └── Info.plist             # macOS configuration
```

## When to Use Platform-Specific Code

✅ Platform-specific APIs (camera, location)
✅ Platform-specific UI adjustments
✅ Platform-specific permissions
✅ Platform-specific performance optimizations

❌ General business logic (belongs in Services)
❌ UI rendering (XAML handles this)

## Using Platform-Specific Code

### Conditional Compilation

```csharp
#if IOS
    // iOS-specific code
#elif ANDROID
    // Android-specific code
#elif WINDOWS
    // Windows-specific code
#elif MACCATALYST
    // macOS-specific code
#endif
```

### Runtime Detection

```csharp
if (DeviceInfo.Platform == DevicePlatform.iOS)
{
    // iOS-specific logic
}
else if (DeviceInfo.Platform == DevicePlatform.Android)
{
    // Android-specific logic
}
```

### Platform-Specific Services

```csharp
// Services/Interfaces/ICameraService.cs
public interface ICameraService
{
    Task<string> TakePhotoAsync();
}

// Platforms/iOS/CameraService.cs
#if IOS
public class CameraService : ICameraService
{
    public async Task<string> TakePhotoAsync()
    {
        // iOS-specific implementation
    }
}
#endif
```

Register conditional:
```csharp
#if IOS
builder.Services.AddSingleton<ICameraService, CameraService>();
#elif ANDROID
builder.Services.AddSingleton<ICameraService, CameraService>();
#endif
```

## Common Platform-Specific Tasks

| Task | Platform | File |
|------|----------|------|
| Permissions | All | `Platforms/[Platform]/` |
| Native UI | All | Platform-specific handlers |
| Files/Storage | All | Use built-in MAUI APIs |
| Camera | iOS, Android | Platform-specific code |
| Location | iOS, Android | Platform-specific code |
| Haptics | iOS, Android | Platform-specific code |

See [MAUI Docs - Platform Specifics](https://learn.microsoft.com/dotnet/maui/platform-integration/overview)
