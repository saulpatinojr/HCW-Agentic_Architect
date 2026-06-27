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
- Added CSV export and BI schema export for ingestion pipelines.
- Added threshold alert panel for high overhead and low partner savings.
- Added lightweight partner adapter health panel.
- Added day/week/month trend panel backed by SQLite history.

## PR-inspired features incorporated

The implementation includes practical equivalents of high-value patterns from reviewed ready-for-review work:

- Per-partner savings breakdown (Codex, Claude, GitHub, Copilot, Antigravity, VS Code)
- Output token savings metric
- Observed TTL bucket mix panel (`5m` and `1h`)
- Overhead metric and live trend snapshot
- Session history feed for recent optimization events

## Added service/model components

- `ContextOptimizationMetricsService` (live MCP stats reader + alert/trend synthesis)
- `DashboardWindowService` (detachable windows)
- `ContextOptimizationHistoryStore` (SQLite persistence and trend aggregation)
- `ContextOptimizationExportService` (JSON, CSV, BI schema export)
- `PartnerAdapterHealthService` (Codex/Claude/GitHub/Copilot/Antigravity/VS Code health checks)
- `CompressionDashboardModels.cs` (snapshot, partner savings, TTL buckets, history, alerts, health, trends)

## Next implementation candidates

- Add configurable thresholds in settings (instead of hardcoded values).
- Add optional remote write pipeline (Prometheus or OTLP) for centralized ops dashboards.
- Add adapter-specific remediation hints for unhealthy partner integrations.
