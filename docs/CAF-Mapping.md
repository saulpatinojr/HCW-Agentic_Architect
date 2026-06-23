## CAF Mapping — Quick Reference

Purpose: map this repository's Terraform/architecture patterns to the Azure Cloud Adoption Framework (CAF) guidance.

- **Scope**: naming, resource groups, landing zones, identity, networking, monitoring, IaC mapping, tagging, and deployment runbook.

1. Naming & Resource Groups
   - Use CAF-inspired short prefixes from docs/project/AGENTS.md (e.g., `rg`, `vn`, `vm`, `aks`).
   - Resource Group pattern: `<org>-<env>-rg-<app>` (example: `hcw-dev-rg-agentic`).

2. Landing Zones
   - Core landing zone resources: identity (Azure AD), networking (vNet+subnets), hub-spoke topology, shared services (log analytics, storage, Key Vault).
   - Map repo modules: `infrastructure/bicep` or `infrastructure/terraform` should provide `network`, `identity`, `shared` modules.

3. Identity & Access
   - Prefer Managed Identities for platform components; use service principals only for CI/CD with least-privilege roles.
   - Centralize role assignment in an `identity` module and record approval/runbook for elevated roles.

4. Networking
   - Hub vNet contains egress and shared services; spokes host workloads.
   - Enforce NSG and UDR examples in `infrastructure` modules; document VPN/ExpressRoute patterns if required.

5. Monitoring & Logging
   - Central Log Analytics workspace per tenant/region; forward diagnostics from App Services/AKS/VMs.
   - Include a minimal Alerting & Workbook README in `docs/` for runbook owners.

6. IaC Mapping
   - Terraform: follow `workspace-config/agents/tf-engineer/AGENTS.md` rules — remote state, locking, provider-agnostic modules.
   - Bicep: use for environment-specific orchestration and ARM-native resources; keep modules small and idempotent.

7. Tagging & Metadata
   - Required tags: `cost-center`, `owner`, `env`, `project`, `created-by`.
   - Enforce tags in CI plan via a validation job.

8. Deployment Runbook (high level)
   - Steps: plan → validate (lint + policy) → apply in non-prod → integration tests → apply in prod with approvals.
   - Remote state: use central storage account with locking (e.g., Azurerm backend + blob lease).

References
- CAF overview: https://learn.microsoft.com/azure/cloud-adoption-framework/overview
- Terraform guidance: see `workspace-config/agents/tf-engineer/AGENTS.md`

Notes
- CI templates included in `docs/ci-templates/` target Windows runners per project decision.
