# Claude / Claude Code Agent Instructions

> Scope: Claude CLI (claude-code) and Claude.ai interactions.
> Inherits from: `../../AGENTS.md` and `../../.claude/CLAUDE.md`

## CLI Setup

```bash
# Install
npm install -g @anthropic-ai/claude-code

# Authenticate
claude auth login

# Run in project root
claude
```

## MCP Server Integration

Claude Code reads MCP server config from `.vscode/mcp.json` or `~/.claude/mcp.json`.

Configured MCP servers for this repo:
- `github` — GitHub API access via `@modelcontextprotocol/server-github`
- `azure` — Azure resource queries (when available)
- `filesystem` — Local file operations

See `.vscode/mcp.json` for full server config.

## Slash Commands (Custom)

```
/deploy-review    → Run az what-if and summarize changes
/naming-check     → Validate all resource names against CAF standard
/agent-sync       → Remind to update AGENTS.md after stack changes
/cost-estimate    → Run az cost estimate for current template
```

## Rules

- Claude reads `CLAUDE.md` first, then falls back to `AGENTS.md`
- MCP tools give Claude live repo state — prefer MCP over file reads where possible
- Never auto-approve destructive operations via MCP
- Keep `.claude/CLAUDE.md` minimal — redirect to `AGENTS.md` for all project context
