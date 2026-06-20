---
applyTo: "**/*.bicep,**/*.bicepparam"
---

# Bicep File Instructions

These instructions apply to all `.bicep` and `.bicepparam` files in this repository.

## Structure Rules

- `main.bicep` is an **orchestrator only** — never declare inline resources here
- Every resource lives in a dedicated module under `infra/bicep/modules/`
- Module folders: `network/`, `identity/`, `storage/`, `keyvault/`, `ai/`, `monitoring/`

## Required Decorators

```bicep
@description('...')  // Required on every param
@secure()            // Required on secrets
@minLength(3)        // Use validation decorators
@maxLength(24)
```

## Required Module Outputs

Every module MUST output:
```bicep
output resourceId string = resource.id
output resourceName string = resource.name
```

## Naming

All resource names via the shared naming function in `infra/bicep/shared/naming.bicep`:
```bicep
module naming '../shared/naming.bicep' = {
  name: 'naming'
  params: { workload: workload, env: env, region: region }
}
```

## Tags

Every resource MUST include:
```bicep
tags: {
  env: env
  owner: ownerEmail
  costCenter: costCenter
  project: projectName
  managedBy: 'bicep'
  createdDate: utcNow('yyyy-MM-dd')
}
```
