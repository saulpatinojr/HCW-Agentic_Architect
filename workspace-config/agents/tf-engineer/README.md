# Terraform Engineer Agent Kit

Senior Terraform engineer agent for designing, writing, reviewing, and operationalizing cloud-agnostic Infrastructure as Code.

## Scope

Invoke for Terraform architecture, code generation, module design, remote state and backend setup, environment separation, CI/CD workflow design, validation, testing, linting, security scanning, and documentation.

## Core Principles

- Provider-agnostic by default — do not assume AWS, Azure, GCP, or any specific backend unless the repository or user specifies it.
- Remote state required for all team and CI/CD environments; local state only for isolated experiments.
- Separate state per environment and per lifecycle boundary.
- Reusable, typed, validated modules over copy-paste configuration.
- Version-pinned providers and modules; commit `.terraform.lock.hcl`.
- Secrets never in code, state examples, comments, or test fixtures.
- Safe, reviewable changes — destructive operations always called out explicitly.

## Files

| File | Purpose |
|---|---|
| `AGENTS.md` | Primary agent instructions (all AI agents) |
| `CLAUDE.md` | Claude-specific working rules |
| `.github/copilot-instructions.md` | GitHub Copilot behavior |
| `.codex/instructions.md` | Codex/OpenAI agent behavior |
| `pack.manifest.json` | Workspace catalog registration |

## Recommended Repository Structure

```
infra/
├── modules/
│   ├── network/
│   ├── compute/
│   ├── data/
│   ├── identity/
│   ├── monitoring/
│   └── security/
└── live/
    ├── dev/
    ├── test/
    └── prod/
```

## Validation Commands

```bash
terraform fmt -recursive
terraform init -backend=false
terraform validate
terraform test
```

## Handoffs

- **AWS Architect** — switch when cloud-specific AWS design decisions are needed
- **Azure Architect** — switch when cloud-specific Azure design decisions are needed
