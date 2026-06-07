# docs/

This directory contains architecture documentation, Statements of Work (SOWs), Architecture Decision Records (ADRs), and runbooks for the HCW Agentic Architect platform.

## Structure (planned)

```
docs/
├── architecture/     # Architecture diagrams and decision records (ADRs)
├── sow/              # Statements of Work and engagement documents
├── runbooks/         # Operational runbooks and incident response
├── naming/           # CAF naming convention reference sheets
└── presentations/    # Customer-facing decks and slide content
```

## ADR Naming Convention

```
docs/architecture/adr-NNNN-<short-title>.md
```

Example: `adr-0001-use-bicep-for-azure-iac.md`

## Key Reference Documents

- Azure CAF Naming: see `/AGENTS.md#azure-caf-naming-convention`
- Security posture: see `/.agents/security.md`
- IaC conventions: see `/.agents/iac.md`
