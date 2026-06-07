# Gemini Agent Instructions

> Scope: Gemini CLI and Google AI Studio interactions.
> Inherits from: `../../AGENTS.md` and `../../GEMINI.md`

## CLI Setup

```bash
# Install
npm install -g @google/gemini-cli

# Authenticate
gemini auth login

# Run in project root
gemini
```

## Memory Management

Gemini CLI has persistent memory across sessions. Manage it actively:

```bash
# View current memory
/memory show

# Refresh after AGENTS.md updates
/memory refresh

# Clear stale context
/memory clear
```

**After any change to `AGENTS.md` or project structure, always run `/memory refresh`.**

## Tools Available

Gemini CLI has access to:
- `read_file` / `write_file` — file operations
- `run_shell_command` — shell execution (use carefully)
- `web_search` — live web queries
- `google_search_grounding` — grounded responses

## Rules

- Run `/memory show` at the start of each new session to verify context
- Run `/memory refresh` after any `AGENTS.md` or structural changes
- Prefer Gemini for research tasks (web_search) and code review
- Never use `run_shell_command` for destructive az or terraform operations
