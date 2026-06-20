# GitHub Agent Instructions

> Scope: All GitHub operations — repos, branches, PRs, Actions workflows, and `gh` CLI usage.
> Inherits from: `../../AGENTS.md`

## Repository Naming Convention

All repos under `github.com/saulpatinojr` follow this pattern:

`<category>-<ProjectName>`

| Category | Purpose | Example |
|---|---|---|
| `Work-` | Client/professional projects | `Work-Azure_Spec_Builder` |
| `HCW-` | Home Cloud Work / lab projects | `HCW-Agentic_Architect` |
| `Personal-` | Personal projects | `Personal-Site_Atlas` |

## Branch Strategy

```
main          ← Protected; no direct pushes
develop       ← Integration branch
feature/      ← feature/<short-description>
fix/          ← fix/<issue-number>-<description>
docs/         ← docs/<what-changed>
chore/        ← chore/<task>
release/      ← release/<version>
```

## gh CLI Patterns

```bash
# Repo operations
gh repo clone saulpatinojr/HCW-Agentic_Architect
gh repo view --web

# Branch
gh repo clone saulpatinojr/<repo>
git checkout -b feature/<name>

# PR workflow
gh pr create --title "feat: <description>" --body-file .github/pull_request_template.md
gh pr view --web
gh pr merge --squash --delete-branch

# Issues
gh issue list
gh issue create --title "<title>" --label "enhancement"

# Actions
gh workflow list
gh run list --limit 10
gh run watch
```

## Commit Message Convention

Follow Conventional Commits:

```
<type>(<scope>): <short description>

[optional body]
[optional footer]
```

Types: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `ci`, `perf`

Examples:
- `feat(bicep): add Key Vault module with private endpoint`
- `fix(terraform): correct NSG rule priority conflict`
- `docs(agents): update CAF naming convention table`
- `chore(scripts): add macOS install script`

## Rules

- Never force push to `main` or `develop`
- All PRs require at least one review before merge
- Squash merge PRs to keep main history clean
- Delete branches after merge
- Never commit: `.env`, `*.local`, `terraform.tfstate`, `AGENTS.local.md`, credentials
