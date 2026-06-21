# AI Architect Agents Project

AI Architect Agents is a local desktop control surface for AI architecture workspace packs. It focuses on making local agent packs discoverable, inspectable, validated, updateable, and safely applied to the active workspace.

**Owner:** [Saul Patino](https://github.com/saulpatinojr)  
**Stack:** .NET MAUI · Windows desktop · C# · MCP · PowerShell · workspace packs

## Quick Start

```powershell
dotnet build src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src\HCWMauiApp\WorkspaceManager.csproj -f net10.0-windows10.0.19041.0
```

## App Responsibilities

- Discover local packs from `workspace-config/agents`.
- Import zipped workspace packs.
- Validate host tools, required files, MCP helper health, and manifest compatibility.
- Preview or apply selected pack changes.
- Show provider groups with local icons and official links.
- Inspect generated workspace files and open local paths.
- Compare local packs against the repository catalog foundation.

## Provider Groups

| Group | Providers |
| --- | --- |
| Cloud Providers | AWS, Azure, Google Cloud, VMware |
| Service Providers | Ansible, Docker, FinOps Foundation, GitHub, Kubernetes, Terraform |
| AI Providers | Claude, Codex, GitHub Copilot |

ISO is intentionally not registered as a provider.

## Workspace Pack Files

Each pack should keep its root files directly under `workspace-config/agents/<pack-id>/`:

- `pack.manifest.json`
- `AGENTS.md`
- `CLAUDE.md`
- `.codex/instructions.md`
- `.github/copilot-instructions.md`

Avoid nested pack roots such as `workspace-config/agents/<pack-id>/<pack-id>/`.

## AI Agent Files

| File | Scope | Readers |
| --- | --- | --- |
| `docs/project/AGENTS.md` | Project agent guidance | Claude, Copilot, Gemini, Codex |
| `docs/project/CLAUDE.md` | Claude project context | Claude Code |
| `docs/project/GEMINI.md` | Gemini project context | Gemini CLI |
| `workspace-config/agents/*/AGENTS.md` | Pack-specific agent behavior | Agent runtimes |
| `workspace-config/agents/*/CLAUDE.md` | Pack-specific Claude behavior | Claude Code |

## Sensitive Values

Do not commit local tenant IDs, subscription IDs, credentials, tokens, or generated secrets. Keep local overrides outside committed pack files.
