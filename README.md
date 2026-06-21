# AI Architect Agents

Version: 0.2

AI Architect Agents is a local .NET MAUI desktop app for composing, validating, updating, and activating AI architecture agent packs in this workspace. The app combines a Windows desktop control surface with pack manifests, MCP helper wiring, provider metadata, and workspace file inspection.

## Current Highlights

- Renamed the desktop experience to AI Architect Agents.
- Added a Windows-first command bar for Scan, Import, Preflight, Preview, Check updates, Update pack, and Apply.
- Added provider groups for Cloud Providers, Service Providers, and AI Providers with local image assets and official links.
- Added structured activity entries, a pack inspector, workspace file shortcuts, MCP helper health, and pack update states.
- Added explicit MAUI image packaging so local PNG/SVG provider assets are included in Windows builds.
- Removed ISO from the provider list because it is not a selected provider.

## Repository Structure

- `docs`: project, application, MAUI, and legacy reference docs.
- `scripts`: bootstrap and workspace management scripts.
- `src`: MAUI app source and unit tests.
- `tests`: test guidance and future integration-test structure.
- `workspace-config`: active agent packs, instructions, skills, hooks, and MCP server assets.

## Run On Windows

```powershell
dotnet build src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
```

The app is configured for unpackaged Windows desktop execution with `.NET 10`.

## Validate

```powershell
dotnet test src\HCWMauiApp.Tests\HCWMauiApp.Tests.csproj
```

Current expected result: all unit tests pass. One existing `xUnit2031` analyzer warning remains in `ToolInstallServiceTests.cs`.

## Provider Assets

Provider icons live under `src/HCWMauiApp/Resources/Images`. The project explicitly includes that folder as `MauiImage` resources, and the UI references Windows-emitted PNG resource names such as `provider_azure.png`.

Selected provider groups:

- Cloud Providers: AWS, Azure, Google Cloud, VMware.
- Service Providers: Ansible, Docker, FinOps Foundation, GitHub, Kubernetes, Terraform.
- AI Providers: Claude, Codex, GitHub Copilot.

## Next Milestones

- Finish visual polish after confirming image rendering on the target desktop.
- Add a dedicated app icon package for Windows shell/taskbar surfaces.
- Expand pack catalog update flows beyond the local repository source.
- Resolve the existing `xUnit2031` analyzer warning.
