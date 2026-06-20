# Claude Code Command: /adr

## Usage
```
/adr <subject>
```

## What this command does
1. Reads `docs/adr/index.md` to get the next ADR number.
2. Reads all `accepted` ADRs whose governing agents relate to <subject>.
3. Invokes the `adr` skill to scaffold the new ADR file.
4. If the subject involves a network, platform, AI, or identity decision,
   also invokes the `drawio` skill to create a companion diagram.
5. Appends the new ADR to `docs/adr/index.md`.
6. Commits both files with the standard commit message.

## What to check before running
- Is this decision genuinely architectural? (Not a config tweak or bug fix.)
- Does an accepted ADR already cover this? (Check index first.)
- Which `.agents/` files govern this domain?

## Example
```
/adr use-container-apps-for-rag-api
```
