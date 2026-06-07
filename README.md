# HCW Agentic Architect Platform

Enterprise Azure IaC + AI agent platform for multitenant SaaS workloads.

**Owner:** [Saul Patino](https://github.com/saulpatinojr)  
**Stack:** Azure · Bicep · Terraform · PowerShell · GitHub Actions · Azure OpenAI · AI Search

---

## Quick Start

### Windows (primary)
```powershell
# 1. Install all CLIs
.\scripts\install_cli.ps1

# 2. Configure auth and local settings
.\scripts\configure_repo.ps1

# 3. Open in VS Code
code .
```

### macOS (Apple Silicon)
```bash
# 1. Install all CLIs
bash scripts/install_cli.sh

# 2. Open in VS Code
code .
```

---

## AI Agent Files

| File | Scope | Readers |
|------|-------|---------|
| `AGENTS.md` | Universal | Claude, Copilot, Gemini, Codex, Antigravity |
| `CLAUDE.md` | Claude / MCP config | Claude Code |
| `GEMINI.md` | Gemini CLI config | Gemini CLI |
| `.github/copilot-instructions.md` | Copilot config | GitHub Copilot |
| `.agents/azure.md` | Azure naming & tagging | All AI tools |
| `.agents/iac.md` | Bicep + Terraform rules | All AI tools |
| `.agents/security.md` | Zero-trust / WAF rules | All AI tools |
| `.agents/ai.md` | RAG + Azure AI rules | All AI tools |
| `.agents/cicd.md` | GitHub Actions rules | All AI tools |

---

## Naming Convention

Pattern: `<resource-type>-<workload>-<environment>-<region>-<instance>`  
Full standard with abbreviations table: see `AGENTS.md`

---

## Sensitive Values

Never committed. Created locally by `configure_repo.ps1`:
```
AGENTS.local.md   <- Azure tenant/subscription IDs, region defaults
```

---

## References

- [CAF Naming Convention](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming)
- [CAF Resource Abbreviations](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations)
- [Azure Naming Rules](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules)
