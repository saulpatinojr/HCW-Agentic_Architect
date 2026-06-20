# Implementation TODO

## Completed (per approved order)

- [x] Phase A: Winget-first installer integration in MAUI activation flow.
- [x] Phase B: Remove legacy `workspace-config/hocb/` directory and references.

## In Progress / Next

- [x] Phase C: Add structured `pack.manifest.json` support (start with `tf-engineer`).
- [x] Phase D: Add manifest validator and clear error reporting.
- [x] Phase E: Merge env/tool/file/MCP requirements across selected packs.
- [x] Phase F: Activation hardening (idempotency, conflict handling, dry-run mode).
- [x] Phase G: Refactor `MainPage.xaml.cs` orchestration into focused services.
- [x] Phase H: Expand system checks to align with `workspace-config/manifest.md` and platform specifics.

## Verification

- [ ] Run build validation after installing a .NET SDK (currently missing in environment).
- [x] Add unit tests for manifest parsing/validation and tool install mapping.
