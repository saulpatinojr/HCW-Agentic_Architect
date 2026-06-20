# GitHub Copilot Agent Instructions

> Scope: GitHub Copilot Chat, Copilot Edits, and Copilot for Azure in VS Code / Antigravity.
> Primary config: `../.github/copilot-instructions.md`
> Inherits from: `../../AGENTS.md`

## VS Code / Antigravity Integration

Copilot reads instructions from:
1. `.github/copilot-instructions.md` — workspace-level instructions (this repo)
2. `.github/instructions/*.instructions.md` — glob-scoped per file type
3. User-level settings in VS Code / Antigravity settings

## Copilot for Azure

With the `GitHub Copilot for Azure` extension installed:

```
@azure What resources are in rg-lz-dev-eus2?
@azure /deploy - trigger deployment workflow
@azure /costs - show cost breakdown
@azure How do I add a private endpoint to this Key Vault?
```

## Inline Completion Context

Copilot uses open files as context. For best results:
- Keep `AGENTS.md` open in a side tab during IaC work
- Open the relevant module file alongside `main.bicep`
- Use descriptive variable and parameter names — Copilot completes based on naming

## Rules

- Use `#file:AGENTS.md` in Copilot Chat to explicitly inject project context
- Use `#file:<path>` to reference specific files in chat
- Copilot Edits: always review diff before accepting — never bulk-accept infrastructure changes
- Use `/explain` on unfamiliar Bicep/Terraform patterns before accepting
