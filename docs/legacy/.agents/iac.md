# IaC Agent (Bicep + Terraform)
# Scope: all infrastructure-as-code generation and review

Follow AGENTS.md as primary source. This file adds IaC workflow detail.

## Tool Boundary
| Responsibility              | Tool      |
|-----------------------------|-----------|
| Azure resource deployment   | Bicep     |
| Landing zone / CAF modules  | Terraform |
| Post-deploy config (OS/app) | Ansible   |
| Secrets management          | Key Vault (referenced by both) |

## Bicep Module Structure
```
infra/bicep/
├── main.bicep               ← Entry point, module calls only
├── main.bicepparam          ← Default params
├── environments/
│   ├── prod.bicepparam
│   └── dev.bicepparam
└── modules/
    ├── networking/
    │   ├── vnet.bicep
    │   └── nsg.bicep
    ├── compute/
    │   └── vm.bicep
    ├── ai/
    │   ├── openai.bicep
    │   └── aisearch.bicep
    ├── security/
    │   └── keyvault.bicep
    └── identity/
        └── managedidentity.bicep
```

## Terraform Structure
```
infra/terraform/
├── main.tf
├── variables.tf
├── outputs.tf
├── providers.tf
├── backend.tf
├── terraform.tfvars         ← Dev defaults (gitignored for prod values)
└── modules/
    └── <module-name>/
        ├── main.tf
        ├── variables.tf
        └── outputs.tf
```

## State Management
- Backend: Azure Storage Account
- Name pattern: `sthcwtfstate<env>001` (no hyphens — storage account rule)
- Container: `tfstate`
- Key: `<workload>/<env>/terraform.tfstate`
- Locking: enabled via Azure Blob lease
