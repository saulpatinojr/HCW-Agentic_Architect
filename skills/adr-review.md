# Skill: ADR Review and Compliance Check
# Trigger: before PR creation, when infra or platform files change, when
#           architecture changes are proposed, or on explicit request.

## Purpose
Ensure that proposed changes do not contradict accepted ADRs. Surface conflicts
early, before implementation, not after.

## When to Run
Run this skill automatically when:
- Any `.bicep`, `.tf`, `.ps1`, or `.agents/*.md` file is modified.
- A new Azure service, model deployment, or network pattern is introduced.
- An agent proposes a design that deviates from established standards.
- The words "change strategy", "new pattern", "replace", or "migrate" appear in a plan.
- A PR is being created for IaC, AI, or platform changes.

## Workflow
1. Read `docs/adr/index.md` to get the list of accepted ADRs.
2. Read ADRs whose governing agents overlap with the proposed change.
3. For each relevant ADR, check:
   - Does the proposed change conform to the decision?
   - Does it depend on something the ADR says should not be used?
   - Does it replace something the ADR says should be used?
4. Produce a compliance summary:
   - ✅ Conforms — no conflicts
   - ⚠️ Deviation — explain what differs and why it may be intentional
   - ❌ Conflict — hard contradiction; must be resolved before proceeding
5. If a conflict exists:
   - Propose an ADR update if the decision itself should change, OR
   - Propose a code/design change to realign with the accepted ADR.
6. Document the review outcome in the PR description or session notes.

## Compliance Summary Template
```markdown
## ADR Compliance Review

| ADR | Title | Status | Finding |
|-----|-------|--------|--------|
| ADR-NNNN | <title> | accepted | ✅ / ⚠️ / ❌ |

### Findings Detail
<For each ⚠️ or ❌, explain the conflict and recommended resolution.>
```

## Conflict Resolution Options
| Finding | Action |
|---------|--------|
| ✅ Conforms | Proceed. No action needed. |
| ⚠️ Deviation | Document the deviation reason in the PR. May need ADR update. |
| ❌ Conflict | Stop. Resolve by updating the design or opening a new ADR to supersede. |

## ADR Update Rule
When a conflict is ❌ and the existing ADR is outdated:
1. Run the `adr` skill to create a new ADR with status `proposed`.
2. Set the `Supersedes` field to the conflicting ADR.
3. Once accepted, mark the old ADR `superseded` and link to the new one.
4. Re-run the review — it should now show ✅.
