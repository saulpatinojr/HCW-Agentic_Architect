# infra/terraform/

This directory contains Terraform configurations using the Azure CAF (Cloud Adoption Framework) Landing Zone pattern.

## Structure (planned)

```
infra/terraform/
├── main.tf                  # Root module — calls sub-modules
├── variables.tf             # Input variable declarations
├── outputs.tf               # Output declarations
├── versions.tf              # Provider and Terraform version constraints
├── terraform.tfvars         # Default values (non-sensitive)
├── envs/
│   ├── dev.tfvars           # Dev environment overrides
│   ├── qa.tfvars            # QA environment overrides
│   └── prod.tfvars          # Prod environment overrides
└── modules/                 # Local Terraform modules
```

## Conventions

- Remote state: Azure Storage Account — `st` prefix, CAF naming, gitignored `.tfbackend`
- CAF naming module: `aztfmod/caf/azurerm` for all resource names
- One `.tfvars` file per environment under `envs/`
- `terraform fmt` enforced before every commit (CI check)
- No hardcoded credentials — use `azurerm` provider with Managed Identity or env vars

## Naming

All resources follow the CAF pattern defined in `/AGENTS.md#azure-caf-naming-convention`.

## Quick Start

```bash
terraform -chdir=infra/terraform init -backend-config=envs/dev.tfbackend
terraform -chdir=infra/terraform plan -var-file=envs/dev.tfvars -out=tfplan
terraform -chdir=infra/terraform apply tfplan
```
