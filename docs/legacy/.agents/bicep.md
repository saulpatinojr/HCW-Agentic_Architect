# Bicep Agent Instructions

> Scope: All `.bicep` and `.bicepparam` files.
> Inherits from: `../../AGENTS.md` and `./../azure.md`

## Module Architecture

All Bicep work follows a strict module-first pattern:

```
infra/bicep/
├── main.bicep              ← Orchestrator only; no inline resources
├── main.bicepparam         ← Environment-specific parameters
├── modules/
│   ├── network/
│   │   ├── vnet.bicep
│   │   └── nsg.bicep
│   ├── identity/
│   │   ├── managed-identity.bicep
│   │   └── role-assignment.bicep
│   ├── storage/
│   │   └── storage-account.bicep
│   ├── keyvault/
│   │   └── key-vault.bicep
│   ├── ai/
│   │   ├── openai.bicep
│   │   ├── ai-search.bicep
│   │   └── content-safety.bicep
│   └── monitoring/
│       ├── log-analytics.bicep
│       └── diagnostics.bicep
└── shared/
    └── naming.bicep        ← Centralized name generation function
```

## Coding Standards

```bicep
// REQUIRED: Always declare targetScope
targetScope = 'resourceGroup'

// REQUIRED: Always use @description decorators
@description('Environment name: dev | tst | stg | prd')
param env string

// REQUIRED: Tags object on every module
param tags object = {
  env: env
  managedBy: 'bicep'
  createdDate: utcNow('yyyy-MM-dd')
}

// REQUIRED: Output the resource ID and name from every module
output resourceId string = resource.id
output resourceName string = resource.name
```

## Commands

```powershell
# Lint
az bicep lint --file infra/bicep/main.bicep

# Build (compile to ARM JSON)
az bicep build --file infra/bicep/main.bicep --outfile infra/arm/main.json

# Validate (dry run)
az deployment group validate `
  --resource-group $RG `
  --template-file infra/bicep/main.bicep `
  --parameters infra/bicep/main.bicepparam

# What-if (preview changes)
az deployment group what-if `
  --resource-group $RG `
  --template-file infra/bicep/main.bicep `
  --parameters infra/bicep/main.bicepparam

# Deploy
az deployment group create `
  --resource-group $RG `
  --template-file infra/bicep/main.bicep `
  --parameters infra/bicep/main.bicepparam `
  --name "deploy-$(Get-Date -Format 'yyyyMMdd-HHmm')"
```

## Rules

- `main.bicep` is an orchestrator ONLY — no inline resource declarations
- Every module is self-contained with its own params, vars, resources, and outputs
- Use `existing` keyword to reference resources not managed in this template
- Use `@secure()` decorator for all sensitive parameters
- Never hardcode location — always pass as parameter defaulting to `resourceGroup().location`
- Key Vault references for all secrets in `.bicepparam` files
