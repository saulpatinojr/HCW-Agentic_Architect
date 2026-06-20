# GitHub Copilot Workspace Instructions

> This file is read automatically by GitHub Copilot Chat in VS Code and Antigravity.
> For full project context, see the root `AGENTS.md`.

## Project: HCW-Agentic_Architect

Azure-first IaC and AI agent scaffolding platform. Owner: Saul Patino (saulpatinojr).

## Quick Reference

- **Primary IaC**: Bicep (modules) + Terraform (state-managed infra)
- **Scripting**: PowerShell 7.4+ (pwsh) — cross-platform
- **Identity**: Managed identities, Entra ID, Key Vault references only
- **Naming**: CAF standard — `<type>-<workload>-<env>-<region>[-<instance>]`
- **Tags**: `env`, `owner`, `costCenter`, `project`, `managedBy`, `createdDate` on every resource

## CAF Naming Quick Reference

| Resource | Abbreviation | Example |
|---|---|---|
| Resource Group | `rg` | `rg-lz-prd-eus2` |
| Virtual Network | `vnet` | `vnet-lz-prd-eus2` |
| Key Vault | `kv` | `kv-rag-dev-eus2-001` |
| Storage Account | `st` | `stragprdeus2001` |
| Azure OpenAI | `aoai` | `aoai-rag-dev-eus2-001` |
| AI Search | `srch` | `srch-rag-dev-eus2-001` |
| Container App | `ca` | `ca-api-dev-eus2-001` |
| Managed Identity | `id` | `id-rag-dev-eus2-001` |
| Log Analytics | `log` | `log-lz-prd-eus2-001` |

## Behavior

- Suggest CAF-compliant names automatically for any new Azure resource
- Default to Bicep modules — never suggest inline resources in `main.bicep`
- Always include tags block in every resource suggestion
- Use `@secure()` on any parameter that holds a secret
- Prefer managed identity auth patterns over connection strings

## Use `#file:AGENTS.md` in Copilot Chat

For detailed project context, always reference:
```
#file:AGENTS.md
```
