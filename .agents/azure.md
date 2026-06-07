# Azure Agent Instructions

> Scope: All Azure resource operations, ARM/Bicep deployments, Entra ID, and Azure-native services.
> Inherits from: `../../AGENTS.md`

## Identity & Context

You are operating as an Azure Cloud Architect assistant for Saul Patino (HCW). All work targets Microsoft Azure with a zero-trust, CAF-aligned posture. Primary workloads include multitenant SaaS, RAG pipelines (Azure OpenAI + AI Search), and enterprise Landing Zone deployments.

## Azure CLI Patterns

```bash
# Always scope to subscription before acting
az account set --subscription $AZURE_SUBSCRIPTION_ID

# Read-only checks (always safe)
az resource list --resource-group $RG --output table
az deployment group validate --resource-group $RG --template-file main.bicep

# Deployments (ask first)
az deployment group create \
  --resource-group $RG \
  --template-file main.bicep \
  --parameters @params.json
```

## CAF Naming Convention

All Azure resources MUST follow the CAF naming standard defined in `../../infra/AGENTS.md`.

Key pattern: `<type>-<workload>-<env>-<region>[-<instance>]`

| Token | Values |
|---|---|
| `<type>` | CAF abbreviation (e.g. `rg`, `vnet`, `st`, `kv`) |
| `<workload>` | Short descriptor (e.g. `lz`, `rag`, `idp`) |
| `<env>` | `dev` / `tst` / `stg` / `prd` |
| `<region>` | Azure short region (e.g. `eus2`, `wus3`, `scus`) |
| `<instance>` | Zero-padded number `001`–`999` (when multiple exist) |

Examples:
- `rg-lz-prd-eus2`
- `kv-rag-dev-eus2-001`
- `st-rag-prd-eus2-001`

## Mandatory Resource Tags

Every resource and resource group MUST include:

```json
{
  "env": "dev | tst | stg | prd",
  "owner": "saul.patino@hcw.com",
  "costCenter": "<cost-center-code>",
  "project": "<project-name>",
  "managedBy": "terraform | bicep | manual",
  "createdDate": "YYYY-MM-DD"
}
```

## Security Guardrails

- **Identity**: Use managed identities exclusively. No service principal passwords.
- **Secrets**: Key Vault references only. Never hardcode credentials, connection strings, or subscription IDs in code.
- **Network**: Private endpoints preferred. No public endpoints without WAF.
- **RBAC**: Least-privilege. Use built-in roles before custom.
- **Diagnostics**: All resources must emit logs to Log Analytics workspace.

## Boundaries

### ✅ Always Safe
- `az * list`, `az * show`, `az * validate`
- Read-only queries, linting, cost estimates

### ⚠️ Ask First
- `az deployment group create`
- `az role assignment create`
- `az keyvault secret set`
- Any delete or purge operation

### 🚫 Never
- Commit subscription IDs, tenant IDs, or secrets to git
- Modify production resource groups directly
- Disable diagnostic settings or security policies
- Use `--no-wait` on destructive operations
