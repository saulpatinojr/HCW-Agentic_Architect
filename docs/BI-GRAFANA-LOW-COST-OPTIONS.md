# BI and Grafana Low-Cost Options

This guide focuses on free/low-cost options for dashboarding context optimization telemetry.

## Recommended tiers

## Tier 1: Free local-first (best default)

- Grafana OSS (self-hosted)
- SQLite datasource plugin or CSV ingestion
- Local files exported from app (`JSON`, `CSV`, `context-optimization-schema.json`)

Cost profile:
- License: free (Grafana OSS)
- Infra: zero if local workstation; low if hosted on a small VM

## Tier 2: Low-cost managed visualizations

- Grafana Cloud Free tier for small workloads
- Push summarized metrics only (avoid high-cardinality raw events)

Cost profile:
- License: free tier, then pay-as-you-grow
- Control cost by ingesting aggregates every 5-15 minutes

## Tier 3: BI tool without premium licenses

- Apache Superset (open source) with SQLite/Postgres backend
- Metabase open source self-hosted

Cost profile:
- License: free
- Infra: small VM/container costs

## Export format contract

The app emits:
- `context-optimization-<timestamp>.csv`
- `context-optimization-<timestamp>.json`
- `context-optimization-schema.json`

Use schema fields to map dashboards consistently across tools.

## Cost controls to keep telemetry cheap

- Aggregate at source: export rollups instead of raw event streams where possible.
- Poll every 5-15 minutes for BI dashboards instead of sub-second intervals.
- Keep retention windows bounded (for example, 30-90 days in SQLite).
- Use one row per snapshot plus one row per partner contribution.
- Avoid uncontrolled labels/dimensions in Grafana/Prometheus (prevents expensive cardinality).

## Suggested starter setup

1. Use SQLite history store as the source of truth.
2. Export CSV on schedule (or from dashboard button).
3. Build Grafana OSS dashboards locally.
4. Move to Grafana Cloud Free only when sharing/collaboration is needed.
