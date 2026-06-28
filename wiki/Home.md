# HCW Agentic Architect

AI Architect Agents is an orchestration-first system for infrastructure delivery.

## What This Project Is For

Agentic Architect is the main orchestrator agent. Its role is to convert architecture intent into safe, governed, and repeatable infrastructure outcomes.

This is the target workflow the app is being built to run:

1. Capture requirements, constraints, and target topology.
2. Convert intent into canonical IaC-ready specifications.
3. Generate and review Terraform/IaC plans through pull-request gates.
4. Enforce security and policy checks before release.
5. Approve, apply, and verify releases through controlled release agents.
6. Deliver cloud-ready infrastructure outcomes across AWS, Azure, and GCP.

## Core Product Narrative

Cloud architecture is the foundation. AI architecture is built on top of that foundation.

The system expands from infrastructure-only concerns into full-stack architecture outcomes:

- Infrastructure layer: compute, storage, network, IAM, Kubernetes, observability.
- Data layer: pipelines, feature stores, vector and quality context.
- Model layer: model selection, tuning, deployment posture.
- Agent layer: MCP/tool orchestration and workflow execution.
- Governance layer: policy checks, safety controls, release guardrails.
- Business outcomes: measurable delivery speed, reliability, and operator trust.

## IaC Agent Ecosystem (Operational View)

The intended ecosystem sequence:

1. Developer asks for infrastructure change.
2. Architect Agent refines requirements and constraints.
3. IaC/Terraform Agent creates plan/state-aware change set.
4. Pull Request gate verifies plan quality and readiness.
5. Security/Policy Agent validates IAM, network, and compliance policies.
6. Release Agent approves and executes controlled apply/deploy.
7. Observability and release gates confirm production readiness.

## Delivery Priorities

### Phase 1

- Canonical manifest model (WHO/HOW/TRUST/TALK).
- Harness adapter layer.
- Source-of-truth generation pipeline.

### Phase 1.5

- Delegation modes (advisory vs implementation).
- Workflow gates and stage checkpoints.
- Security profiles and hook bundles.
- Quality thresholds plus circuit breaker.

### Phase 2+

- Multi-host workspace targeting.
- Federation and swarm orchestration.

## Documentation Map

- [Wiki Overview](README.md)
- [MAUI Complete Index](MAUI-COMPLETE-INDEX.md)
- [MAUI Quick Reference](MAUI-QUICK-REFERENCE.md)
- [MAUI Architecture](MAUI-ARCHITECTURE.md)
- [CAF Mapping](CAF-Mapping.md)
- [Context Optimization Features](CONTEXT-OPTIMIZATION-FEATURES.md)
- [BI and Grafana Low-Cost Options](BI-GRAFANA-LOW-COST-OPTIONS.md)

## Repository Wiki Source

This wiki folder is the canonical documentation source in this repository and is synchronized to the GitHub Wiki.
