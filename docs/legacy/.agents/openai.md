# OpenAI / Codex Agent Instructions

> Scope: ChatGPT, Codex CLI, and OpenAI API interactions.
> Inherits from: `../../AGENTS.md`

## CLI Setup

```bash
# Install
npm install -g @openai/codex

# Authenticate
export OPENAI_API_KEY=$(cat ~/.secrets/openai_api_key)

# Run in project
codex --approval-mode suggest
```

## Approval Mode

| Mode | Behavior | Use When |
|---|---|---|
| `suggest` | Read-only; shows suggestions | Default/exploration |
| `auto-edit` | Edits files; no shell | Refactoring |
| `full-auto` | Full autonomy | CI pipelines only |

**Default for this repo: `suggest`**

## Context Files

Codex CLI reads `AGENTS.md` automatically from the project root and walks subdirectories. No extra config needed.

## Rules

- Always run in `suggest` mode locally unless explicitly overridden
- Never use `full-auto` mode on infrastructure files
- API key lives in `~/.secrets/openai_api_key` (never in repo)
- Use `--no-git` flag when testing outside a git repo
