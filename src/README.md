# Source

Source tree for AI Architect Agents.

## Projects

```text
src/
├── HCWMauiApp/         # .NET MAUI desktop app
└── HCWMauiApp.Tests/   # Unit tests for app services and models
```

## App Focus

`HCWMauiApp` is not a generic MAUI scaffold. It is the desktop control surface for this workspace:

- Discover and import workspace packs.
- Validate pack manifests and host requirements.
- Inspect provider metadata and official links.
- Preview or apply generated workspace files.
- Verify MCP helper availability.
- Prepare local pack update workflows.

## Build

```powershell
dotnet build src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
```

## Test

```powershell
dotnet test src\HCWMauiApp.Tests\HCWMauiApp.Tests.csproj
```

## Key Implementation Areas

- `MainPage.xaml`: desktop layout and visual system.
- `MainPage.xaml.cs`: UI orchestration and event handling.
- `Models/WorkspaceModels.cs`: pack, provider, folder, update, and activity models.
- `Services/ProviderRegistryService.cs`: provider grouping and image/link metadata.
- `Resources/Images`: local app and provider image assets.
