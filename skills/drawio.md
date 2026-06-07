# Skill: Architecture Diagram (Draw.io)
# Trigger: when a new or updated architecture diagram is needed, or when
#           an ADR references a diagram that does not yet exist.

## Purpose
Create, update, or validate `.drawio` architecture diagrams using the Draw.io
MCP server. Diagrams are the visual complement to ADRs and architecture docs.

## MCP Dependency
This skill requires the `drawio` MCP server to be active.
See `.vscode/mcp.json` and `.mcp.json` for configuration.
MCP server: `drawio-mcp-server` via `uvx`.

## Diagram Types
| Type   | Prefix   | Use case                                      |
|--------|----------|-----------------------------------------------|
| `arch` | `arch-`  | Platform and solution architecture overviews  |
| `net`  | `net-`   | Network topology, hub-spoke, private endpoints|
| `seq`  | `seq-`   | Sequence flows, RAG pipelines, event flows    |
| `dep`  | `dep-`   | Deployment topology, AKS/Container Apps layout|
| `iam`  | `iam-`   | Identity, RBAC, trust, Entra ID flows         |

## Naming Convention
```
docs/diagrams/<type>-<subject>-<version>.drawio
Examples:
  docs/diagrams/arch-hcw-platform-v1.drawio
  docs/diagrams/net-hub-spoke-eastus2-v1.drawio
  docs/diagrams/iam-entra-rag-rbac-v1.drawio
  docs/diagrams/seq-rag-pipeline-v1.drawio
  docs/diagrams/dep-container-apps-v1.drawio
```

## Workflow
1. Identify diagram type and subject from the context or ADR.
2. Confirm output path using the naming convention above.
3. Use the `drawio` MCP server to generate the diagram.
4. Save to `docs/diagrams/<filename>.drawio`.
5. Review the diagram visually in VS Code using the Draw.io Integration extension
   (`hediet.vscode-drawio`) before committing.
6. Update `docs/diagrams/README.md` index table with the new file.
7. If the diagram was triggered by an ADR, update the "Related Diagram" field.
8. Commit with message: `docs(diagrams): add <filename>`.

## Diagram Content Standards
Every diagram should make explicit:
- Trust boundaries (use dashed lines or shaded regions)
- Network / VNet boundaries
- Tenant boundaries in multitenant designs
- Control plane vs data plane flows
- Private vs public paths (use different line styles)
- Azure resource abbreviations consistent with `AGENTS.md`
- Numbered callouts for complex flows

## VS Code Extension
For manual editing and review:
- Extension: `hediet.vscode-drawio`
- Open any `.drawio` file in VS Code or Antigravity to edit visually

## Azure Architecture Icon Set
Use the official Microsoft Azure architecture icon set:
- Download from https://aka.ms/AzureIcons
- Import into Draw.io via Extras > Edit Diagram or shape library

## Review Checklist
Before committing a diagram:
- [ ] Named correctly and saved to `docs/diagrams/`
- [ ] Index in `docs/diagrams/README.md` updated
- [ ] Relevant ADR updated with diagram reference
- [ ] Trust and network boundaries are explicit
- [ ] Azure abbreviations match `AGENTS.md` standards
- [ ] Reviewed visually in VS Code Draw.io extension
