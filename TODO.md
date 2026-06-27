# Implementation TODO

## Completed

- [x] Phase A: Winget-first installer integration in MAUI activation flow.
- [x] Phase B: Remove legacy `workspace-config/hocb/` directory and references.
- [x] Phase C: Add structured `pack.manifest.json` support.
- [x] Phase D: Add manifest validator and clear error reporting.
- [x] Phase E: Merge env/tool/file/MCP requirements across selected packs.
- [x] Phase F: Activation hardening with idempotency, conflict handling, and preview mode.
- [x] Phase G: Refactor `MainPage.xaml.cs` orchestration into focused services.
- [x] Phase H: Expand system checks to align with `workspace-config/manifest.md`.
- [x] Redesign desktop UI as AI Architect Agents with provider groups, pack inspector, activity feed, and workspace files.
- [x] Add explicit MAUI image packaging for provider/logo assets.
- [x] Remove ISO from the provider registry.
- [x] Resolve the `xUnit2031` analyzer warning in `ToolInstallServiceTests.cs`.
- [x] Fix the Windows MAUI build failure in `ContextOptimizationHistoryStore`.
- [x] Update SQLite dependencies to clear the `SQLitePCLRaw.lib.e_sqlite3` vulnerability warning.
- [x] Replace obsolete dashboard `DisplayAlert` calls with `DisplayAlertAsync`.

## In Progress / Next

- [ ] Confirm provider and app logo rendering on the target desktop after the `.png` resource-name fix.
- [ ] Add a proper Windows app icon package for title bar, taskbar, and executable surfaces.
- [ ] Improve responsive behavior below the default desktop size.
- [ ] Add visual states for provider link buttons and workspace-file launch results.
- [ ] Expand pack update workflows to support remote catalogs after the local repo catalog is stable.

## Verification

- [x] Build Windows MAUI target with `.NET 10`.
- [x] Run MAUI unit tests.
- [x] Verify MAUI image resources are emitted into the Windows build output.
- [ ] Manual UI check on the target desktop after relaunch.
