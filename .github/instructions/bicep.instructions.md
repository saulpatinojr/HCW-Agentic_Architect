---
applyTo: "**/*.bicep"
---
# Bicep Agent Instructions

Follow AGENTS.md. Additional Bicep-specific rules:

1. Every param must have `@description()` decorator
2. Every output must have `@description()` decorator
3. Use `existing` keyword for cross-resource references — no string interpolation for resource IDs
4. Target scope: `resourceGroup` unless explicitly set to `subscription` or `managementGroup`
5. API versions: use latest stable (check aka.ms/bicep-types)
6. Naming: follow CAF pattern from AGENTS.md — `<abbr>-<workload>-<env>-<region>-<instance>`
7. No `any()` type casts — resolve types explicitly
8. Lint before committing: `az bicep lint --file <file>`
