# Skill: ADR Authoring
# Trigger: any session where a meaningful architectural decision is made,
#           changed, superseded, or needs to be validated against prior choices.

## Purpose
Capture every significant technical decision in a structured, versioned ADR
in `docs/adr/`. ADRs are the durable architectural memory of this repo.
All agents read ADRs before proposing or implementing changes.

## Trigger Conditions
Run this skill when any of the following are true:
- A choice between two or more real alternatives was made.
- An accepted platform standard is intentionally changed.
- A new external dependency, Azure service, or technology pattern is introduced.
- A cost, security, resilience, or compliance tradeoff materially changes the design.
- A diagram is needed to explain or validate the decision.

## Workflow
1. Determine next ADR number from `docs/adr/index.md`.
2. Create `docs/adr/<NNNN>-<kebab-title>.md` using the template below.
3. Populate every field — no placeholder text.
4. Link the relevant `.agents/` files under "Governing Agents."
5. If a diagram is warranted, invoke the `drawio` skill and reference the output file.
6. Append the new ADR to the index table in `docs/adr/index.md`.
7. Commit with message: `docs(adr): add ADR-NNNN <title>`.

## ADR Template
```markdown
# ADR-NNNN: <Title>

| Field             | Value                                                    |
|-------------------|----------------------------------------------------------|
| Status            | proposed                                                 |
| Date              | YYYY-MM-DD                                               |
| Owner             | Saul Patino (@saulpatinojr)                              |
| Governing Agents  | <comma-separated list from .agents/>                     |
| Related Diagram   | docs/diagrams/<filename>.drawio (if applicable)          |
| Supersedes        | ADR-XXXX (if applicable)                                 |
| Superseded By     | —                                                        |

## Context
<What situation, constraint, requirement, or problem drives this decision?>

## Decision
<State the decision clearly in one or two sentences.>

## Rationale
<Why was this option chosen?>

## Alternatives Considered
| Option | Reason Not Chosen |
|--------|-------------------|
| ...    | ...               |

## Consequences
### Positive
- ...

### Negative / Trade-offs
- ...

### Neutral
- ...

## Implementation Notes
<Key steps, sequencing, or agents that must act on this decision.>

## MCP Context Used
| Server         | What was queried                             |
|----------------|----------------------------------------------|
| github         | PR/issue/code context (if applicable)        |
| azure          | Resource state (if applicable)               |
| drawio         | Diagram generated (if applicable)            |

## Related ADRs
- ADR-XXXX: <title>
```

## Status Lifecycle
| Status     | Meaning                                          |
|------------|--------------------------------------------------|
| proposed   | Under review, not yet binding                    |
| accepted   | Finalized and binding on all future work         |
| superseded | Replaced by a later ADR                          |
| deprecated | No longer relevant to the current system         |
| rejected   | Considered but not adopted — kept for audit trail|

## Governing Agent Rules
- `azure.md` — subscription, resource group, naming, region, policy decisions
- `iac.md` — Bicep vs Terraform, module structure, state, tool boundary decisions
- `ai.md` — model selection, RAG architecture, tenant isolation, evaluation strategy
- `security.md` — identity, secrets, exposure, compliance, WAF decisions
- `networking.md` — topology, private endpoints, DNS, hybrid connectivity decisions
- `cicd.md` — branch strategy, pipeline design, release flow decisions
- `containers.md` — AKS vs Container Apps, image strategy decisions
- `finops.md` — SKU, tier, shared vs isolated service cost decisions
- `docs.md` — documentation standard, diagram convention decisions
- `powershell.md` — automation pattern, script design decisions

## Diagram Rule
If the decision affects network topology, AI architecture, platform topology,
identity flow, or deployment flow — create a diagram via the `drawio` skill.
Reference it in the "Related Diagram" field of the ADR.
