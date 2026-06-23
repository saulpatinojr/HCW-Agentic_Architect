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
- Added `--stats-json` CLI mode so the app can read `optimization_stats` directly.
- Added persisted stats storage (`optimization-stats.json`) to retain metrics across process restarts.

## Dashboard mockups inside the MAUI app

- Added in-app compact dashboard preview strip on the main page.
- Added full dashboard page mockup at:
  - `src/HCWMauiApp/Features/ContextOptimization/ContextOptimizationDashboardPage.xaml`
- Added detachable window support so users can pop out dashboards.
- Replaced mocked dashboard data with direct MCP stats reads by invoking:
  - `python workspace-config/mcp-servers/token-compressor/server.py --stats-json`
- Added auto-refresh polling and JSON export in the dashboard page.

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

- Add CSV export alongside JSON export.
- Add SQLite-backed long-term history and day/week/month aggregations.
- Add alert thresholds for high overhead and low savings.
- Add partner-specific health cards for Codex, Claude, GitHub, Copilot, Antigravity, and VS Code adapters.
