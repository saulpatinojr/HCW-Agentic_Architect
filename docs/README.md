# AI Architect Agents Documentation

Documentation hub for the AI Architect Agents workspace.

## Quick Navigation

### Project

- [Project overview](project/README.md)
- [Agent guidance](project/AGENTS.md)
- [Claude guidance](project/CLAUDE.md)
- [Gemini guidance](project/GEMINI.md)

### Application

- [MAUI architecture](application/MAUI-ARCHITECTURE.md)
- [MAUI complete index](MAUI-COMPLETE-INDEX.md)
- [MAUI quick reference](MAUI-QUICK-REFERENCE.md)
- [App source guide](../src/HCWMauiApp/README.md)
- [Tests guide](../src/HCWMauiApp.Tests/README.md)

### Workspace Config

- [Workspace manifest](../workspace-config/manifest.md)
- [AI architect overview](../workspace-config/ai-architect.md)
- [Orchestration guardrails](../workspace-config/instructions/orchestration-guardrails.md)
- [MCP scaffold skill](../workspace-config/skills/scaffold-mcp-server.md)

### Legacy Reference

Historical architecture and infrastructure material now lives under `docs/legacy`. Keep new app and workspace-pack documentation in the active docs listed above.

## Current App Surface

The desktop app is a Windows-first MAUI control room for local AI architecture packs. It includes:

- Command bar: Scan, Import, Preflight, Preview, Check updates, Update pack, Apply.
- Provider groups: Cloud Providers, Service Providers, AI Providers.
- Pack inspector with provider links, guidance links, tools, and MCP servers.
- Workspace files panel for generated local artifacts.
- Structured activity feed for startup, validation, MCP, update, and apply events.

## Documentation Rules

- Root-level docs should describe the product and operational workflow.
- `src/HCWMauiApp` docs should describe app implementation details.
- `workspace-config` docs should describe pack authoring, agent instructions, and MCP behavior.
- Legacy docs should not be updated unless intentionally preserving historical context.
