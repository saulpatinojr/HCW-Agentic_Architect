# Security Agent (Zero-Trust / WAF)
# Scope: security posture, identity, access, and WAF rules

Follow AGENTS.md as primary source.

## Zero-Trust Principles Applied
1. **Verify explicitly** — always authenticate and authorize using all available data points
2. **Use least privilege** — limit access with Just-In-Time and Just-Enough-Access
3. **Assume breach** — minimize blast radius, segment access, encrypt everything

## Identity Rules
- No service principal passwords — use certificates or managed identities
- Conditional Access: require MFA for all admin operations
- PIM: activate privileged roles Just-In-Time, max 8-hour sessions
- No Global Admin for day-to-day work — use targeted roles

## Network Security
- Default deny on all NSGs — explicit allow rules only
- Private endpoints (`pep-`) for all PaaS services (OpenAI, AI Search, Key Vault, Storage)
- No public IP on VMs unless explicitly justified and approved
- Azure Firewall (`afw-`) as single egress point per hub VNet

## Key Vault Rules
- No secrets in code, config files, or environment variables committed to repo
- Managed identity authentication to Key Vault only
- Soft delete + purge protection enabled on all Key Vaults
- Separate Key Vaults per environment (prod/dev/qa)

## WAF Rules
- Azure Front Door WAF policy (`fdfp-`) in Prevention mode for prod
- OWASP 3.2 ruleset + custom rules for each workload
- Rate limiting: 1000 req/5min per IP on all public endpoints
- Geo-filtering: document allowlist per workload
