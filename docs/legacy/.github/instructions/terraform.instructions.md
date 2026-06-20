---
applyTo: "**/*.tf,**/*.tfvars"
---

# Terraform File Instructions

These instructions apply to all `.tf` and `.tfvars` files in this repository.

## Structure Rules

- `main.tf` at root level: provider config and module calls only
- All resources live inside modules under `infra/terraform/modules/`
- Each module must have: `main.tf`, `variables.tf`, `outputs.tf`

## Required Variables

Every module MUST accept:
```hcl
variable "env" {
  description = "Environment: dev | tst | stg | prd"
  type        = string
  validation {
    condition     = contains(["dev","tst","stg","prd"], var.env)
    error_message = "env must be dev, tst, stg, or prd"
  }
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
```

## Naming

Use the local naming convention:
```hcl
locals {
  name_prefix = "${var.resource_type}-${var.workload}-${var.env}-${var.region}"
}
```

## Sensitive Values

```hcl
output "secret_value" {
  value     = azurerm_key_vault_secret.example.value
  sensitive = true  # REQUIRED for secrets
}
```

## State

- Remote state in Azure Storage only — never local state in production
- State file key: `<workload>/<env>.terraform.tfstate`
- Never commit `terraform.tfstate`, `terraform.tfstate.backup`, `.terraform/`
