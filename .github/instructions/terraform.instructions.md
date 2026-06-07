---
applyTo: "**/*.tf"
---
# Terraform Agent Instructions

Follow AGENTS.md. Additional Terraform-specific rules:

1. Run `terraform fmt` before every commit
2. All resources must include the tags block from AGENTS.md tagging standard
3. Use `azurerm` provider >= 3.100.0
4. Remote state backend: azurerm (storage account `st` + env)
5. No hardcoded location strings — use `var.location`
6. Module source: prefer `aztfmod/caf/azurerm` for landing zone resources
7. Naming: use `azurecaf_name` resource or follow AGENTS.md pattern manually
8. `terraform validate` and `terraform plan` must pass before PR
