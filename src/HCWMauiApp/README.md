# Workspace Manager

Desktop MAUI app for inspecting a local workspace, importing packs, checking required tools, and applying selected packs into the workspace config.

## What It Does

- Scans the repository for `workspace-config/agents/*`
- Imports a pack from a `.zip`
- Runs a local environment and tool check
- Validates pack manifests
- Merges requirements from the selected packs
- Writes the workspace MCP configuration

## Main Screen

- `Scan workspace` refreshes the local pack list
- `Import workspace pack` adds a new pack from a zip file
- `Run system check` reports missing tools or config issues
- `Apply selected packs` performs the workspace update
- `Dry run` previews changes without writing files
- `Include helper MCP` keeps the local helper MCP in the generated config

## Core Services

- `WorkspaceCatalogService` discovers packs and imports archives
- `WorkspaceSystemCheckService` reports the current host/tool state
- `WorkspaceActivationService` validates, merges, and applies selected packs
- `WorkspaceMcpConfigBuilderService` builds the MCP config payload
- `WorkspaceWriterService` writes updated workspace files
- `ToolInstallService` preflights required CLI tools
- `PackManifestService` validates `pack.manifest.json`
- `ManifestRequirementsMergeService` combines requirements across packs

## Run

```bash
dotnet build src/HCWMauiApp/WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src/HCWMauiApp/WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
```

## Notes

- The app is configured for an unpackaged Windows desktop run target.
- The UI is intentionally operational: workspace status, selected packs, and activity log are all on one screen.
