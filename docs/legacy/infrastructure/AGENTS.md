# Infrastructure Agent Instructions

> Scope: All files under `infra/` — Bicep modules, Terraform configs, ARM templates.
> Overrides: Extends root `../AGENTS.md`. Rules here take precedence for IaC files.
> Also see: `../.agents/bicep.md` and `../.agents/terraform.md`

---

## CAF Naming Convention Standard

> Derived from:
> - [Azure CAF Naming Best Practices](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming)
> - [CAF Abbreviation Recommendations](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations)
> - [Azure Resource Naming Rules](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules)
> - [Naming Convention and Tagging](https://learn.microsoft.com/en-us/community/content/azure-resources-naming-convention-and-tagging)

---

### Master Pattern

```
<type>-<workload>-<env>-<region>[-<instance>]
```

| Token | Description | Allowed Values |
|---|---|---|
| `<type>` | CAF resource type abbreviation | See table below |
| `<workload>` | Short app/service name (2–8 chars, lowercase) | `lz`, `rag`, `api`, `idp`, `hub`, `spk` |
| `<env>` | Deployment environment | `dev` `tst` `stg` `prd` |
| `<region>` | Azure region short code | `eus2` `wus3` `scus` `neu` `weu` |
| `<instance>` | Zero-padded 3-digit index | `001`–`999` (only when multiple of same type exist) |

**General Rules:**
- All lowercase
- Hyphens as separators (exception: storage accounts and some resources cannot have hyphens — see per-resource rules below)
- Max length varies by resource type — check per-resource rules
- No special characters except hyphens
- No consecutive hyphens
- No leading or trailing hyphens

---

### Region Short Codes

| Azure Region | Short Code |
|---|---|
| East US | `eus` |
| East US 2 | `eus2` |
| West US | `wus` |
| West US 2 | `wus2` |
| West US 3 | `wus3` |
| South Central US | `scus` |
| North Central US | `ncus` |
| Central US | `cus` |
| North Europe | `neu` |
| West Europe | `weu` |
| UK South | `uks` |
| Southeast Asia | `sea` |
| Australia East | `aue` |

---

### CAF Resource Abbreviations & Naming Rules

#### Management & Organization

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Management Group | `mg` | `mg-<workload>` | 90 | No region token |
| Subscription | `sub` | `sub-<workload>-<env>` | 64 | No region token |
| Resource Group | `rg` | `rg-<workload>-<env>-<region>` | 90 | No instance |

Examples:
- `mg-landingzone`
- `sub-lz-prd`
- `rg-lz-prd-eus2`
- `rg-rag-dev-eus2`

---

#### Networking

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Virtual Network | `vnet` | `vnet-<workload>-<env>-<region>` | 64 | |
| Subnet | `snet` | `snet-<workload>-<env>-<region>` | 80 | |
| Network Security Group | `nsg` | `nsg-<workload>-<env>-<region>` | 80 | |
| Application Security Group | `asg` | `asg-<workload>-<env>-<region>` | 80 | |
| Route Table | `rt` | `rt-<workload>-<env>-<region>` | 80 | |
| Virtual WAN | `vwan` | `vwan-<workload>-<env>-<region>` | 80 | |
| Virtual Hub | `vhub` | `vhub-<workload>-<env>-<region>` | 80 | |
| VPN Gateway | `vpng` | `vpng-<workload>-<env>-<region>` | 80 | |
| Application Gateway | `agw` | `agw-<workload>-<env>-<region>-<instance>` | 80 | |
| Azure Firewall | `afw` | `afw-<workload>-<env>-<region>` | 80 | |
| Azure Firewall Policy | `afwp` | `afwp-<workload>-<env>-<region>` | 80 | |
| Load Balancer (internal) | `lbi` | `lbi-<workload>-<env>-<region>` | 80 | |
| Load Balancer (public) | `lbe` | `lbe-<workload>-<env>-<region>` | 80 | |
| Public IP | `pip` | `pip-<workload>-<env>-<region>-<instance>` | 80 | |
| Private DNS Zone | `pdnsz` | `pdnsz-<service>.privatelink.<region>` | 63/label | Follow privatelink pattern |
| Private Endpoint | `pep` | `pep-<workload>-<env>-<region>-<instance>` | 80 | |
| DNS Resolver | `dnspr` | `dnspr-<workload>-<env>-<region>` | 80 | |
| Bastion Host | `bas` | `bas-<workload>-<env>-<region>` | 80 | |
| NAT Gateway | `ng` | `ng-<workload>-<env>-<region>` | 80 | |

Examples:
- `vnet-hub-prd-eus2`
- `snet-rag-dev-eus2`
- `nsg-api-dev-eus2`
- `pip-agw-prd-eus2-001`
- `pep-kv-dev-eus2-001`

---

#### Compute

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Virtual Machine (Windows) | `vm` | `vm<workload><env><instance>` | 15 | **No hyphens**; Windows NetBIOS limit |
| Virtual Machine (Linux) | `vm` | `vm-<workload>-<env>-<region>-<instance>` | 64 | Hyphens allowed on Linux |
| VM Scale Set | `vmss` | `vmss-<workload>-<env>-<region>` | 64 | |
| Availability Set | `avail` | `avail-<workload>-<env>-<region>` | 80 | |
| Managed Disk | `disk` | `disk-<workload>-<env>-<region>-<instance>` | 80 | |
| Container Instance | `ci` | `ci-<workload>-<env>-<region>-<instance>` | 63 | |
| Container App | `ca` | `ca-<workload>-<env>-<region>-<instance>` | 32 | |
| Container App Environment | `cae` | `cae-<workload>-<env>-<region>` | 60 | |
| AKS Cluster | `aks` | `aks-<workload>-<env>-<region>-<instance>` | 63 | |

Examples:
- `vmlzdev001` (Windows VM)
- `vm-lz-dev-eus2-001` (Linux VM)
- `ca-api-dev-eus2-001`
- `cae-rag-dev-eus2`
- `aks-lz-prd-eus2-001`

---

#### Storage

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Storage Account | `st` | `st<workload><env><region><instance>` | 24 | **No hyphens**; lowercase alphanumeric only; globally unique |
| Storage Container | n/a | `<descriptive-name>` | 63 | Lowercase + hyphens |
| Data Lake Storage Gen2 | `dls` | `dls<workload><env><region><instance>` | 24 | **No hyphens** |
| Managed Disk | `disk` | `disk-<workload>-<env>-<region>-<instance>` | 80 | |

Examples:
- `stragdeveus2001` (Storage Account)
- `dlsrawprdeus2001` (Data Lake)

---

#### Databases

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| SQL Server | `sql` | `sql-<workload>-<env>-<region>` | 63 | Globally unique |
| SQL Database | `sqldb` | `sqldb-<workload>-<env>-<region>` | 128 | |
| SQL Elastic Pool | `sqlep` | `sqlep-<workload>-<env>-<region>` | 128 | |
| Cosmos DB Account | `cosmos` | `cosmos-<workload>-<env>-<region>` | 44 | Globally unique |
| Redis Cache | `redis` | `redis-<workload>-<env>-<region>` | 63 | Globally unique |
| PostgreSQL Server | `psql` | `psql-<workload>-<env>-<region>` | 63 | Globally unique |
| MySQL Server | `mysql` | `mysql-<workload>-<env>-<region>` | 63 | Globally unique |
| Azure SQL Managed Instance | `sqlmi` | `sqlmi-<workload>-<env>-<region>` | 63 | |

Examples:
- `sql-rag-dev-eus2`
- `cosmos-rag-dev-eus2`
- `redis-api-dev-eus2`

---

#### Identity & Security

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Key Vault | `kv` | `kv-<workload>-<env>-<region>-<instance>` | 24 | Globally unique; no consecutive hyphens |
| Managed Identity (User) | `id` | `id-<workload>-<env>-<region>-<instance>` | 128 | |
| Azure AD App Registration | `app` | `app-<workload>-<env>` | 120 | No region token typically |
| App Service Plan | `asp` | `asp-<workload>-<env>-<region>` | 40 | |

Examples:
- `kv-rag-dev-eus2-001`
- `kv-lz-prd-eus2-001`
- `id-rag-dev-eus2-001`
- `id-lz-prd-eus2-001`

---

#### AI & Machine Learning

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Azure OpenAI | `aoai` | `aoai-<workload>-<env>-<region>-<instance>` | 64 | Globally unique |
| AI Search | `srch` | `srch-<workload>-<env>-<region>-<instance>` | 60 | Globally unique |
| Azure AI Services | `ais` | `ais-<workload>-<env>-<region>-<instance>` | 64 | |
| Content Safety | `cs` | `cs-<workload>-<env>-<region>-<instance>` | 64 | |
| Machine Learning Workspace | `mlw` | `mlw-<workload>-<env>-<region>-<instance>` | 260 | |
| Cognitive Services | `cog` | `cog-<workload>-<env>-<region>-<instance>` | 64 | |
| Bot Service | `bot` | `bot-<workload>-<env>` | 42 | |

Examples:
- `aoai-rag-dev-eus2-001`
- `srch-rag-prd-eus2-001`
- `cs-rag-dev-eus2-001`
- `mlw-rag-dev-eus2-001`

---

#### Monitoring & Management

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Log Analytics Workspace | `log` | `log-<workload>-<env>-<region>-<instance>` | 63 | |
| Application Insights | `appi` | `appi-<workload>-<env>-<region>` | 260 | |
| Azure Monitor Action Group | `ag` | `ag-<workload>-<env>-<region>` | 260 | |
| Dashboard | `dash` | `dash-<workload>-<env>` | 160 | |
| Automation Account | `aa` | `aa-<workload>-<env>-<region>` | 50 | |
| Recovery Services Vault | `rsv` | `rsv-<workload>-<env>-<region>` | 50 | |

Examples:
- `log-lz-prd-eus2-001`
- `appi-api-dev-eus2`

---

#### Integration & Messaging

| Resource | Abbreviation | Pattern | Max Length | Notes |
|---|---|---|---|---|
| Service Bus Namespace | `sb` | `sb-<workload>-<env>-<region>` | 50 | Globally unique |
| Service Bus Queue | `sbq` | `sbq-<workload>` | 260 | |
| Service Bus Topic | `sbt` | `sbt-<workload>` | 260 | |
| Event Hub Namespace | `evhns` | `evhns-<workload>-<env>-<region>` | 50 | Globally unique |
| Event Hub | `evh` | `evh-<workload>-<env>-<region>` | 256 | |
| Event Grid Topic | `evgt` | `evgt-<workload>-<env>-<region>` | 50 | |
| API Management | `apim` | `apim-<workload>-<env>-<region>` | 50 | Globally unique |
| Logic App | `logic` | `logic-<workload>-<env>-<region>` | 43 | |
| Function App | `func` | `func-<workload>-<env>-<region>-<instance>` | 60 | Globally unique |
| App Service | `app` | `app-<workload>-<env>-<region>-<instance>` | 60 | Globally unique |

Examples:
- `sb-rag-dev-eus2`
- `apim-api-prd-eus2`
- `func-rag-dev-eus2-001`

---

### Mandatory Resource Tags

Every Azure resource and resource group deployed from this repo MUST include all of these tags:

```json
{
  "env":         "dev | tst | stg | prd",
  "owner":       "<upn-or-email>",
  "costCenter":  "<cost-center-code>",
  "project":     "<project-short-name>",
  "managedBy":   "terraform | bicep | manual | ansible",
  "createdDate": "YYYY-MM-DD"
}
```

| Tag | Purpose | Example |
|---|---|---|
| `env` | Lifecycle environment | `prd` |
| `owner` | Accountable person/team UPN | `saul.patino@contoso.com` |
| `costCenter` | FinOps cost attribution | `CC-1234` |
| `project` | Project identifier | `rag-platform` |
| `managedBy` | IaC tool that owns this resource | `bicep` |
| `createdDate` | ISO 8601 creation date | `2026-06-07` |

---

### Globally Unique Resources Checklist

These resources have **globally unique** name requirements — run availability check before deploying:

- Storage Accounts (`st*`)
- Data Lake Storage (`dls*`)
- Key Vault (`kv-*`)
- Azure OpenAI (`aoai-*`)
- AI Search (`srch-*`)
- Cosmos DB (`cosmos-*`)
- Redis Cache (`redis-*`)
- PostgreSQL (`psql-*`)
- MySQL (`mysql-*`)
- SQL Server (`sql-*`)
- API Management (`apim-*`)
- Service Bus Namespace (`sb-*`)
- Event Hub Namespace (`evhns-*`)
- Function App (`func-*`)
- App Service (`app-*`)

```powershell
# Availability check (storage account example)
$nameAvail = az storage account check-name --name 'stragdeveus2001' | ConvertFrom-Json
if (-not $nameAvail.nameAvailable) { Write-Error "Name taken: $($nameAvail.reason)" }
```

---

### IaC Boundaries

| Responsibility | Terraform | Bicep |
|---|---|---|
| Landing Zone foundation (VNets, hubs, firewall) | ✅ | ❓ review |
| Management Group + Policy assignments | ✅ | – |
| Remote state infrastructure | ✅ | – |
| Application-layer resources (AI, Storage, KV) | – | ✅ |
| Module reuse across teams | – | ✅ |
| GitHub Actions CI/CD deployments | Both | Both |

---

## Key Commands (IaC)

```powershell
# Bicep
az bicep lint --file infra/bicep/main.bicep
az deployment group what-if --resource-group $RG --template-file infra/bicep/main.bicep

# Terraform
terraform -chdir=infra/terraform fmt -recursive
terraform -chdir=infra/terraform validate
terraform -chdir=infra/terraform plan -out=tfplan
terraform -chdir=infra/terraform apply tfplan
```
