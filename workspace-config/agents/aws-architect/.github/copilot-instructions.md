# GitHub Copilot Instructions: AWS Certified Architect

Use this file to adapt Copilot behavior for the `aws-architect` agent in this repository. Follow the repository-level `AGENTS.md` as primary source.

Rules
- Keep IaC examples pinned and safe.
- Flag public exposure, IAM scope wideners, and destructive operations.
- Use draw.io for diagrams when requested.

Validation
- Recommend `terraform fmt`, `terraform validate`, and provider-specific checks when producing IaC.
