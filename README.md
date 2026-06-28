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
- Updated SQLite dependencies and dashboard export alerts so the Windows MAUI build is warning-free.

## Repository Structure

- `wiki`: canonical documentation source for project and MAUI references.
- `.ci`: CI configuration assets for GitHub Actions and Azure DevOps.
- `scripts`: bootstrap and workspace management scripts.
- `src`: MAUI app source and unit tests.
- `tests`: test guidance and future integration-test structure.
- `workspace-config`: active agent packs, instructions, skills, hooks, and MCP server assets.

## GitHub Wiki

Documentation now lives in `wiki` and is organized to mirror GitHub Wiki pages.

- Start page source: `wiki/Home.md`
- Wiki navigation source: `wiki/_Sidebar.md`

To publish these pages to the repository wiki, push/copy the files from `wiki` into
the GitHub wiki repository (`HCW-Agentic_Architect.wiki.git`).

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

Current expected result: all unit tests pass without analyzer warnings.

## Provider Assets

Provider icons live under `src/HCWMauiApp/Resources/Images`. The project explicitly includes that folder as `MauiImage` resources, and the UI references Windows-emitted PNG resource names such as `provider_azure.png`.

Selected provider groups:

- Cloud Providers: AWS, Azure, Google Cloud, VMware.
- Service Providers: Ansible, Docker, FinOps Foundation, GitHub, Kubernetes, Terraform.
- AI Providers: Claude, Codex, GitHub Copilot.

## Pack Distribution

Agent pack ZIPs are published with stable filenames via GitHub Pages. Clients should
query the published manifest to detect new versions and verify checksums.

- Manifest source: `teams/packs.manifest.json`
- Manifest URL: `https://saulpatinojr.github.io/HCW-Agentic_Architect/packs.manifest.json`

This allows cross-platform clients to compare local installed versions against
the current published versions without changing file names.

## Next Milestones

- Finish visual polish after confirming image rendering on the target desktop.
- Add a dedicated app icon package for Windows shell/taskbar surfaces.
- Expand pack catalog update flows beyond the local repository source.
