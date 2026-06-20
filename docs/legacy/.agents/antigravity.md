# Antigravity Agent Instructions

> Scope: Antigravity IDE setup, configuration, and AI agent usage within Antigravity.
> Inherits from: `../../AGENTS.md`

## What is Antigravity

Antigravity is a VS Code-compatible IDE that supports the VS Code extension marketplace, `.vscode/settings.json`, and CodeX integration via `.vsix` export/import.

## Setup

```bash
# Install Antigravity CLI
# Windows (winget)
winget install Antigravity.Antigravity

# macOS (brew)
brew install --cask antigravity

# Open this repo
antigravity .

# Or use the CLI alias
ag .
```

## Extension Installation

Antigravity reads VS Code marketplace directly. Install extensions from:
- Settings → Extensions → Search marketplace
- Or via command line: `antigravity --install-extension <extension-id>`

For CodeX/OpenAI extensions exported from VS Code:
```bash
# Export from VS Code
code --list-extensions > extensions.txt
# Install in Antigravity
cat extensions.txt | xargs -I {} antigravity --install-extension {}
```

## Settings Compatibility

`.vscode/settings.json` is fully compatible with Antigravity. No migration needed.

## MCP Integration

Antigravity supports MCP servers via `.vscode/mcp.json` — same format as VS Code with Copilot.

## Rules

- Keep `.vscode/settings.json` IDE-agnostic (works in both VS Code and Antigravity)
- Use workspace-level settings in `.vscode/` — never rely on machine-level IDE settings for project behavior
- MCP server tokens go in environment variables, never in `mcp.json`
