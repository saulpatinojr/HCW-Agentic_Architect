# HCW Workspace Manager Documentation

Documentation hub for the HCW Workspace Manager platform, organized by domain for MAUI app development.

## Quick Navigation

### 📋 Project

- [AGENTS.md](project/AGENTS.md) - Agent configurations and references
- [CLAUDE.md](project/CLAUDE.md) - Claude tool configurations
- [GEMINI.md](project/GEMINI.md) - Gemini tool configurations
- [Project Overview](project/README.md) - Main project details

### 🏗️ Architecture

- [Architecture Review Guidelines](architecture/architecture-review.md) - Review best practices
- [Draw.io Guidance](architecture/drawio.md) - Diagram and visualization reference
- **ADR (Architecture Decision Records)**
  - [ADR Overview](architecture/adr/adr.md)
  - [ADR Review Process](architecture/adr/adr-review.md)

### 🚀 Infrastructure

- [Infrastructure Agents](infrastructure/AGENTS.md) - IaC agent configs
- [Bicep Modules](infrastructure/bicep/README.md) - Azure Bicep documentation
- [Terraform](infrastructure/terraform/README.md) - Terraform documentation

### 📱 Application

- [Application Source](application/README.md) - Application code documentation

## Complete Structure

```
docs/
├── README.md              # This file
├── project/               # Core project and agent configurations
│   ├── AGENTS.md
│   ├── CLAUDE.md
│   ├── GEMINI.md
│   └── README.md
├── architecture/          # Architecture decisions, diagrams, reviews
│   ├── architecture-review.md
│   ├── drawio.md
│   └── adr/
│       ├── adr.md
│       └── adr-review.md
├── infrastructure/        # IaC and cloud infrastructure
│   ├── AGENTS.md
│   ├── bicep/
│   │   └── README.md
│   └── terraform/
│       └── README.md
└── application/           # Application-specific documentation
    └── README.md
```

## ADR (Architecture Decision Record) Naming

```
docs/architecture/adr/adr-NNNN-<short-title>.md
```

Example: `adr-0001-use-bicep-for-azure-iac.md`

## System Files (Not Here)

Tool integration files remain in their system locations:

- `.github/instructions/` - GitHub instruction files
- `.github/copilot-instructions.md` - VS Code Copilot config
- `.agents/` - Agent configurations
- `.claude/commands/` - Claude command configs

## Key References

- **Azure CAF Naming**: See [Project AGENTS.md](project/AGENTS.md#caf-naming-quick-reference)
- **Security Posture**: See `/.agents/security.md`
- **IaC Conventions**: See `/.agents/iac.md`
