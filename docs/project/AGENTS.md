# AGENTS.md — HCW Workspace Manager Platform
# Owner: Saul Patino (@saulpatinojr) | Repo: HCW-WorkspaceManager
# All AI tools (Claude, Copilot, Gemini, Codex, Antigravity) read this file.
# Tool-specific files are thin delegates that point back here.

## Project Context
Enterprise Azure IaC + AI agent platform. Primary concerns:
- Multitenant SaaS on Azure (Entra ID, Azure OpenAI, AI Search, Content Safety)
- Bicep modules + Terraform CAF Landing Zones
- RAG pipelines and AI agent orchestration
- Zero-trust / WAF-mapped security posture
- PowerShell automation and GitHub Actions CI/CD

## Repo Structure
```
HCW-WorkspaceManager/
├── AGENTS.md                          ← This file (universal)
├── CLAUDE.md                          ← Delegates to AGENTS.md
├── GEMINI.md                          ← Delegates to AGENTS.md
├── .github/
│   ├── copilot-instructions.md        ← Delegates to AGENTS.md
│   └── instructions/
│       ├── bicep.instructions.md      ← applyTo **/*.bicep
│       ├── terraform.instructions.md  ← applyTo **/*.tf
│       └── powershell.instructions.md ← applyTo **/*.ps1
├── .agents/
│   ├── azure.md                       ← Azure resource agent rules
│   ├── iac.md                         ← IaC (Bicep/Terraform) agent rules
│   ├── security.md                    ← Zero-trust / WAF agent rules
│   ├── ai.md                          ← AI/RAG agent rules
│   └── cicd.md                        ← GitHub Actions agent rules
├── infra/
│   ├── bicep/                         ← Bicep modules and main templates
│   └── terraform/                     ← Terraform CAF configs
├── src/                               ← Application source code
├── scripts/
│   ├── install_cli.ps1                ← Windows CLI installer (winget)
│   ├── install_cli.sh                 ← macOS/Linux CLI installer (brew)
│   └── configure_repo.ps1             ← Repo bootstrap + auth config
├── docs/                              ← Architecture docs, SOWs, ADRs
├── .vscode/
│   ├── settings.json
│   └── extensions.json
├── .gitignore
└── README.md
```

## Key Commands
```bash
# Bicep
az bicep build --file infra/bicep/main.bicep
az bicep lint --file infra/bicep/main.bicep
az deployment group create --resource-group $RG --template-file infra/bicep/main.bicep --parameters @infra/bicep/main.bicepparam

# Terraform
terraform -chdir=infra/terraform init
terraform -chdir=infra/terraform plan -out=tfplan
terraform -chdir=infra/terraform apply tfplan

# PowerShell
pwsh ./scripts/<script>.ps1

# GitHub CLI
gh repo view saulpatinojr/HCW-WorkspaceManager
gh pr create --fill
gh workflow run <workflow-name>

# VS Code CLI
code .
code --install-extension <extension-id>

# Antigravity CLI
antigravity open .
```

## Azure CAF Naming Convention
### Pattern
```
<resource-type>-<workload>-<environment>-<region>-<instance>
```
### Components
| Component       | Values                                          | Notes                              |
|-----------------|-------------------------------------------------|------------------------------------|
| resource-type   | See abbreviations table below                   | Always first                       |
| workload        | Short project/service name (e.g. hcw, rag, id) | Lowercase, no spaces               |
| environment     | prod / dev / qa / stage / test                  | Omit for global-scoped resources   |
| region          | eastus2 / westus / westeu / ustx                | Short form from CAF                |
| instance        | 001, 002 … 999                                  | Zero-padded 3 digits               |

### Delimiter Rules
- Use **hyphen `-`** between components for most resources
- **No hyphens** for Storage Accounts and Container Registries (alphanumeric only, lowercase)
- **No hyphens** for Key Vault names longer than 24 chars — shorten workload token

### Key Abbreviations (CAF Official)
| Resource                        | Abbreviation |
|---------------------------------|--------------|
| Resource group                  | rg           |
| Management group                | mg           |
| Virtual network                 | vnet         |
| Subnet                          | snet         |
| Network security group          | nsg          |
| Public IP address               | pip          |
| Load balancer (external)        | lbe          |
| Load balancer (internal)        | lbi          |
| Application gateway             | agw          |
| Firewall                        | afw          |
| Firewall policy                 | afwp         |
| VPN gateway                     | vpng         |
| Virtual network gateway         | vgw          |
| Private endpoint                | pep          |
| Virtual machine                 | vm           |
| VM scale set                    | vmss         |
| Managed disk (OS)               | osdisk       |
| Managed disk (data)             | disk         |
| Container registry              | cr           |
| AKS cluster                     | aks          |
| Container apps                  | ca           |
| Container apps environment      | cae          |
| Function app                    | func         |
| App Service plan                | asp          |
| Web app                         | app          |
| Static web app                  | stapp        |
| Storage account                 | st           |
| Key vault                       | kv           |
| Managed identity                | id           |
| Log Analytics workspace         | log          |
| Application Insights            | appi         |
| Azure OpenAI Service            | oai          |
| AI Search                       | srch         |
| Azure Machine Learning          | mlw          |
| Content safety                  | cs           |
| Document intelligence           | di           |
| Language service                | lang         |
| Bot service                     | bot          |
| Azure Data Factory              | adf          |
| Event Hubs namespace            | evhns        |
| Service Bus namespace           | sbns         |
| API management                  | apim         |
| Logic app                       | logic        |
| Recovery Services vault         | rsv          |
| Automation account              | aa           |
| Azure Bastion                   | bas          |
| SQL server                      | sql          |
| SQL database                    | sqldb        |
| Cosmos DB                       | cosmos       |
| Azure Cache for Redis           | redis        |

### Examples (HCW Platform)
```
rg-hcw-prod-eastus2-001
vnet-hcw-prod-eastus2-001
snet-hcw-prod-eastus2-001
kv-hcw-prod-eastus2-001
oai-hcw-prod-001
srch-hcw-prod
log-hcw-prod-eastus2-001
appi-hcw-prod-eastus2-001
ca-hcw-rag-prod-001
cae-hcw-prod-eastus2-001
sthcwprod001                   ← storage account (no hyphens)
crhcwprod001                   ← container registry (no hyphens)
id-hcw-rag-prod-eastus2-001
aks-hcw-prod-eastus2-001
```

### Tagging Standard (Mandatory on all resources)
```hcl
tags = {
  environment = "prod"          # prod | dev | qa | stage | test
  workload    = "hcw"           # short project token
  owner       = "saul.patino"   # responsible engineer
  costCenter  = "CLOUD-001"     # FinOps cost center
  project     = "HCW-Workspace"  # full project name
  managedBy   = "terraform"     # terraform | bicep | manual | arm
  createdDate = "2026-06-07"    # ISO 8601
}
```

## Code Conventions
### Bicep
- One module per resource type in `infra/bicep/modules/`
- No inline resource declarations in `main.bicep` — all via module calls
- Use `@description()` decorator on every param and output
- Params file: `main.bicepparam` per environment

### Terraform
- CAF module pattern: `module "naming" { source = "aztfmod/caf/azurerm" }`
- Remote state: Azure Storage Account (`st` + environment)
- One `.tfvars` file per environment
- `terraform fmt` before every commit

### PowerShell
- Use `#Requires -Version 7.0`
- Verb-Noun naming: `Get-AzResourceGroup`, `Set-HcwConfig`
- No hardcoded credentials — use `Get-AzKeyVaultSecret` or env vars
- Use `[CmdletBinding()]` and `param()` blocks on all functions

### Security Boundaries
#### ✅ AI agents may do autonomously
- Read any file, run `az` read-only commands, lint, format, plan
- Create feature branches, draft PRs
- Run `terraform plan` and `az bicep build`

#### ⚠️ AI agents must ask before doing
- `az deployment` commands (any write to Azure)
- Role assignments and policy definitions
- Merging to `main` or `develop`
- Deleting any resource group or resource

#### 🚫 AI agents must never do
- Commit `.env`, credentials, subscription IDs, or tenant IDs
- `git push --force` to protected branches
- Modify production resource groups directly
- Store secrets in code — Key Vault references only

## Local-Only Config
Sensitive values live in `AGENTS.local.md` (gitignored). Never commit that file.
Template for first-time setup:
```
AZURE_TENANT_ID=<your-tenant-id>
AZURE_SUBSCRIPTION_ID=<your-subscription-id>
AZURE_SUBSCRIPTION_NAME=<your-subscription-name>
AZURE_DEFAULT_REGION=eastus2
GITHUB_ORG=saulpatinojr
```
