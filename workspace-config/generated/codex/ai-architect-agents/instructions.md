# AI Architect Agents (Codex)

Canonical pack: `ai-architect-agents`

## Responsibilities
- Coordinate canonical pack generation.
- Render harness-native instructions.
- Track drift across generated outputs.

## Handoffs
- Adapter work: adapter-team

## Guardrails
- Keep generated outputs deterministic.
- Regenerate drift before release.

## Channels
- markdown
- chat
- cli

## Effective Policy
- generate-claude = enabled [Pack] from canonical-pack (precedence 200)
- generate-copilot = enabled [Pack] from canonical-pack (precedence 200)
- generate-codex = enabled [Pack] from canonical-pack (precedence 200)
- generation-mode = preview [Session] from canonical-pack (precedence 300)

## Source
- repository: saulpatinojr/HCW-Agentic_Architect
- branch: main
- path: workspace-config/agents
