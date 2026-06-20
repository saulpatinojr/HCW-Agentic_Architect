# CI/CD Agent (GitHub Actions)
# Scope: workflow files, branch protection, deployment pipelines

Follow AGENTS.md as primary source.

## Branch Strategy
| Branch       | Purpose                    | Protection         |
|--------------|----------------------------|-----------------|
| main         | Production deployments     | PR + 1 review + CI |
| develop      | Integration branch         | PR + CI            |
| feature/*    | Feature development        | CI on push         |
| hotfix/*     | Emergency production fixes | PR + 1 review + CI |
| release/*    | Release candidates         | PR + 2 reviews     |

## Workflow Naming
```
.github/workflows/
├── ci.yml                   ← Lint, validate, test on PR
├── cd-dev.yml               ← Deploy to dev on merge to develop
├── cd-prod.yml              ← Deploy to prod on merge to main
├── bicep-validate.yml       ← az bicep build + lint
├── terraform-plan.yml       ← terraform fmt + validate + plan
└── security-scan.yml        ← MSDO / tfsec / checkov
```

## Secrets in GitHub Actions
- Use GitHub Environments for prod/dev separation
- Store Azure credentials as: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
- Use OIDC (federated credentials) — no client secrets in GitHub Secrets
- Never echo secrets in workflow logs — use `::add-mask::`

## Required CI Checks (must pass before merge)
- `bicep-lint` — az bicep lint on all changed .bicep files
- `terraform-validate` — terraform validate + fmt check
- `security-scan` — checkov or tfsec with CRITICAL=fail
- `ps-lint` — PSScriptAnalyzer on all changed .ps1 files
