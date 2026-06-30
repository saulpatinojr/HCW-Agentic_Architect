# Global Copilot CLI Instructions

These instructions apply to every Copilot CLI session on this machine.

## Plan Mode Hook

When working in Plan Mode (user messages prefixed with `[[PLAN]]`), invoke the `plan-mode-reviewer` subagent via the `task` tool **only once the plan is ready to implement** — meaning the scope, approach, and todos are defined well enough that a reviewer can give meaningful feedback and implementation could begin with only minor polish.

Do NOT call the reviewer while the plan is still forming, ambiguous, or blocked on clarifying questions. In those cases, first resolve open questions with the user (via `ask_user` or follow-up turns) and let the plan stabilize before requesting review.

When the plan IS ready to implement, invoke the reviewer automatically — do not ask for permission first.

### When to trigger the reviewer

Trigger review when EITHER of these is true:

**A. "Ready to implement" signal (preferred).** You have concluded the plan is ready to implement — i.e., if the user said "go", you would begin executing without needing to do more planning work. This is an objective self-assessment: all design decisions are made, all todos are written, no blocking ambiguity remains.

**B. ≥ 80% complete heuristic (fallback).** If you're uncertain whether the plan is "ready to implement," estimate completeness. Trigger review when ALL of these hold:
- `plan.md` has been saved to the session workspace.
- Scope, approach, and todos are clearly defined.
- You have no outstanding clarifying questions for the user.
- You estimate the plan is ≥ 80% done and would only need polish, not rework, before implementation.

If neither condition is met, hold off — gather clarifications first, iterate on the plan, and trigger review on a later turn once the plan stabilizes.

The user may also use the phrase **"ready to implement"** (or similar: "plan is done", "looks good, review it") as an explicit signal that overrides your own assessment and forces the reviewer call.

### How to invoke

Call the `task` tool with:
- `agent_type: "plan-mode-reviewer"`
- `mode: "sync"` (wait for feedback before responding to the user)
- `prompt`: include the full contents of `plan.md`, a brief statement of the user's original request, and any relevant context (repo, key files, constraints, decisions already made).

### After receiving feedback

In your response to the user:
- Give a 2–4 sentence summary of the plan.
- Summarize the reviewer's key findings concisely (do not copy the critique verbatim).
- Note which findings you'd recommend adopting vs. setting aside, with brief rationale.
- Ask whether to revise the plan or proceed as-is.

### Skip conditions

Skip the reviewer call entirely if:
- The plan is trivial (single-file, single-line change) and review would be pure overhead.
- The user has explicitly told you to skip review in this session.
