# Terraform Agent Instructions

> Scope: All `.tf` and `.tfvars` files.
> Inherits from: `../../AGENTS.md` and `./../azure.md`

## Module Architecture

```
infra/terraform/
├── main.tf                 ← Root module: providers + module calls only
├── variables.tf            ← All input variable declarations
├── outputs.tf              ← All output declarations
├── terraform.tfvars        ← Non-sensitive defaults (committed)
├── terraform.tfvars.local  ← Sensitive/local overrides (gitignored)
├── versions.tf             ← Provider version constraints
├── backend.tf              ← Remote state config (Azure Storage)
└── modules/
    ├── network/
    ├── identity/
    ├── storage/
    ├── keyvault/
    ├── ai/
    └── monitoring/
```

## Provider Configuration

```hcl
terraform {
  required_version = ">= 1.9.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "rg-tfstate-prd-eus2"
    storage_account_name = "sttfstateprdeus2001"
    container_name       = "tfstate"
    key                  = "<workload>.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
  use_oidc = true  # Prefer OIDC/managed identity auth
}
```

## Commands

```powershell
# Initialize
terraform init -upgrade

# Format all files
terraform fmt -recursive

# Validate
terraform validate

# Plan (always save plan file)
terraform plan -out=tfplan -var-file=terraform.tfvars

# Apply saved plan only
terraform apply tfplan

# Targeted operations (ask first)
terraform plan -target=module.network -out=tfplan
terraform apply tfplan

# State inspection (read-only, always safe)
terraform state list
terraform state show <resource>
terraform output
```

## Rules

- Never run `terraform apply` without a saved plan file
- Never use `terraform destroy` without explicit user confirmation
- All modules must expose `tags` as an input variable
- Remote state in Azure Storage — never local state in production
- Use `sensitive = true` on all secret outputs
- `terraform.tfvars.local` is gitignored — never commit sensitive values
- Use `moved` blocks instead of manual state manipulation
