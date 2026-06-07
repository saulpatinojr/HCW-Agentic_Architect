# Azure Resource Agent
# Scope: all Azure resource creation, naming, and tagging

Follow AGENTS.md as primary source. This file adds Azure-specific detail.

## Naming Validation Checklist
Before generating any Azure resource name, verify:
- [ ] Abbreviation matches CAF table in AGENTS.md
- [ ] Workload token is lowercase, no spaces, ≤ 10 chars
- [ ] Environment token is one of: prod / dev / qa / stage / test
- [ ] Region token uses short CAF form (eastus2, westus, westeu, ustx)
- [ ] Instance is zero-padded 3 digits (001)
- [ ] Storage accounts and container registries have NO hyphens
- [ ] Total length is within Azure resource-type limit

## Scoping Rules
| Scope        | Uniqueness required within         |
|--------------|------------------------------------|
| Global       | All of Azure (DNS-based resources) |
| Subscription | Your subscription                  |
| Resource group | The resource group               |
| Resource     | The parent resource                |

## Mandatory Tags
All resources must include: environment, workload, owner, costCenter, project, managedBy, createdDate
See AGENTS.md tagging standard for values.

## Entra ID Conventions
- App registrations: `app-<workload>-<purpose>` (e.g. `app-hcw-ragapi`)
- Service principals: match app registration name
- Managed identities: `id-<workload>-<role>-<env>-<region>-<instance>`
- Groups: `grp-<scope>-<role>` (e.g. `grp-hcw-devs`, `grp-hcw-readers`)
