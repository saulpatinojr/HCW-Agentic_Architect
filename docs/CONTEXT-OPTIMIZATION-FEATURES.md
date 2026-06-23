# Context Optimization Feature Rollout

This document tracks the context optimization capabilities added to the app for partner workflows (Codex, Claude, GitHub, Copilot, Antigravity, VS Code).

## Delivered in this iteration

## Phase 1: MCP runtime and dependency foundation

- Upgraded helper MCP server startup to `mcp.run()`.
- Added `headroom-ai` dependency in `workspace-config/mcp-servers/token-compressor/requirements.txt`.
- Expanded MCP tools with sanitized naming and compatibility:
  - `read_compressed_file` (compatibility)
  - `compress_context`
  - `retrieve_context`
  - `optimization_stats`

## Dashboard mockups inside the MAUI app

- Added in-app compact dashboard preview strip on the main page.
- Added full dashboard page mockup at:
  - `src/HCWMauiApp/Features/ContextOptimization/ContextOptimizationDashboardPage.xaml`
- Added detachable window support so users can pop out dashboards.

## PR-inspired features incorporated

The implementation includes practical equivalents of high-value patterns from reviewed ready-for-review work:

- Per-partner savings breakdown (Codex, Claude, GitHub, Copilot, Antigravity, VS Code)
- Output token savings metric
- Observed TTL bucket mix panel (`5m` and `1h`)
- Overhead metric and live trend snapshot
- Session history feed for recent optimization events

## Added service/model components

- `ContextOptimizationMetricsService` (mocked live metrics generator for UI iteration)
- `DashboardWindowService` (detachable windows)
- `CompressionDashboardModels.cs` (snapshot, partner savings, TTL buckets, history)

## Next implementation candidates

- Wire `ContextOptimizationMetricsService` to `optimization_stats()` from the MCP runtime over local transport.
- Add export actions (JSON/CSV) for dashboard snapshots.
- Add historical persistence (SQLite) for trend timelines.
- Add alerts for high overhead and low savings thresholds.
