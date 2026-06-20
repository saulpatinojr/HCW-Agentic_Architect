# Skill: Architecture Review
# Trigger: new workload design, significant platform change, pre-deployment
#           review, or explicit review request.

## Purpose
Produce a structured, multi-agent architecture review that evaluates a design
against Azure CAF, zero-trust, IaC, AI/RAG, FinOps, and documentation standards.

## Workflow
1. Read the design from relevant files, ADRs, diagrams, and agent context.
2. Route each review concern to the appropriate specialist agent:
   | Concern | Agent |
   |---------|-------|
   | Platform topology, naming, subscription | `azure.md` |
   | IaC code and deployment patterns | `iac.md` |
   | AI/RAG architecture and safety | `ai.md` |
   | Zero-trust, identity, secrets, WAF | `security.md` |
   | Network topology, private endpoints | `networking.md` |
   | Container hosting choices | `containers.md` |
   | Cost and SKU optimization | `finops.md` |
   | Pipeline and release design | `cicd.md` |
   | Automation and scripts | `powershell.md` |
   | Docs, ADRs, diagrams | `docs.md` |
3. Run ADR review (`adr-review` skill) to surface any decision conflicts.
4. Identify gaps that require new ADRs and queue them.
5. Produce the Architecture Review Report (template below).

## Architecture Review Report Template
```markdown
# Architecture Review: <Subject>

Date: YYYY-MM-DD
Owner: <name>
Agents consulted: <list>

## Summary
<2-3 sentence assessment: is this design ready, needs work, or blocked?>

## Findings by Domain

### Azure Platform
- ✅ / ⚠️ / ❌ <finding>

### IaC
- ✅ / ⚠️ / ❌ <finding>

### AI / RAG
- ✅ / ⚠️ / ❌ <finding>

### Security / Zero-Trust
- ✅ / ⚠️ / ❌ <finding>

### Networking
- ✅ / ⚠️ / ❌ <finding>

### Containers
- ✅ / ⚠️ / ❌ <finding>

### FinOps
- ✅ / ⚠️ / ❌ <finding>

### CI/CD
- ✅ / ⚠️ / ❌ <finding>

## ADR Compliance
<Output from adr-review skill>

## Required ADRs
| Decision | Priority | Assigned Agent |
|----------|----------|----------------|
| ...      | ...      | ...            |

## Recommended Next Steps
1. ...
2. ...
```

## Escalation Rule
If any finding is ❌, the review is blocked until resolved.
⚠️ findings may proceed with documented deviation rationale.
✅ findings require no further action.
