# infra/bicep/modules/

This directory contains reusable Bicep modules for the HCW Agentic Architect platform.

## Convention

- One module file per Azure resource type
- Filename: `<caf-abbreviation>.bicep` (e.g., `kv.bicep`, `oai.bicep`, `ca.bicep`)
- Every module must expose `name`, `location`, and `tags` as parameters
- Use `@description()` decorator on every `param` and `output`
- No inline resource declarations in `main.bicep` — consume modules only

## Module Catalog (planned)

| Module | Resource | CAF Abbrev |
|--------|----------|------------|
| `rg.bicep` | Resource Group | `rg` |
| `vnet.bicep` | Virtual Network | `vnet` |
| `kv.bicep` | Key Vault | `kv` |
| `oai.bicep` | Azure OpenAI | `oai` |
| `srch.bicep` | AI Search | `srch` |
| `ca.bicep` | Container App | `ca` |
| `cae.bicep` | Container Apps Environment | `cae` |
| `id.bicep` | Managed Identity | `id` |
| `log.bicep` | Log Analytics Workspace | `log` |
| `appi.bicep` | Application Insights | `appi` |
| `st.bicep` | Storage Account | `st` |
| `apim.bicep` | API Management | `apim` |

## Naming

All resources follow the CAF pattern defined in `/AGENTS.md#azure-caf-naming-convention`.
