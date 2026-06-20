# src/

This directory contains application source code for the HCW Workspace Manager platform.

## Planned Structure

```
src/
├── api/          # API layer (Azure Functions, Container Apps)
├── agents/       # AI agent orchestration code
├── rag/          # RAG pipeline components (chunking, indexing, retrieval)
├── shared/       # Shared utilities and helpers
└── tests/        # Unit and integration tests
```

## Guidelines

- All AI/agent code follows rules in `.agents/ai.md`
- Security boundaries defined in `.agents/security.md` apply to all source code
- No credentials or secrets in source — use Azure Key Vault references
- See `AGENTS.md` for full conventions
