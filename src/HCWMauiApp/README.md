# AI Architect Agents MAUI App

Windows desktop MAUI app for inspecting local workspace packs, validating requirements, checking updates, and applying selected AI architecture agent packs.

## Main Screen

- `Scan` refreshes local packs from `workspace-config/agents`.
- `Import` adds a zipped workspace pack.
- `Preflight` validates tools, required files, MCP helper state, and manifest compatibility.
- `Preview` runs activation without file writes.
- `Check updates` compares local packs with catalog metadata.
- `Update pack` replaces a selected pack after validation.
- `Apply` writes selected pack outputs to the local workspace.

The screen also includes:

- Run state summary.
- Provider groups with official links.
- Workspace pack list.
- Pack inspector.
- Structured activity feed.
- Workspace files panel.

## Core Services

- `WorkspaceCatalogService` discovers packs and imports archives.
- `WorkspaceSystemCheckService` reports host/tool state.
- `WorkspaceActivationService` validates, merges, and applies selected packs.
- `WorkspaceMcpConfigBuilderService` builds MCP config payloads.
- `WorkspaceWriterService` writes updated workspace files.
- `ToolInstallService` preflights required CLI tools.
- `PackManifestService` validates `pack.manifest.json`.
- `ManifestRequirementsMergeService` combines requirements across packs.
- `ProviderRegistryService` supplies provider groups, icons, colors, and official URLs.

## Image Assets

Provider and app logo files live in `Resources/Images`.

The project explicitly includes those files as `MauiImage` resources. On Windows, MAUI emits scaled PNG assets, so runtime references use names such as:

- `ai_architect_agents.png`
- `provider_azure.png`
- `provider_aws.png`
- `provider_gcp.png`

Do not add ISO back to `ProviderRegistryService`; it is not a selected provider.

## Run

```powershell
dotnet build src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
```

## Notes

- The app is configured for unpackaged Windows desktop execution.
- The app name and window title are `AI Architect Agents`.
- Default window sizing is tuned for the expanded three-column desktop layout.
