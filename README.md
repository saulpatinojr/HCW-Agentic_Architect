# HCW Workspace Manager

Version: 0.1

HCW Workspace Manager is a workspace orchestration project that combines a .NET MAUI desktop app with workspace packs, MCP server wiring, and infrastructure-oriented guidance for cloud and AI workflows.

## v0.1 Highlights

- Added MAUI-based Workspace Manager under src/HCWMauiApp.
- Implemented winget-first CLI preflight installer flow.
- Added system check and team orchestration flow to workspace activation.
- Introduced workspace-config layout for agents, instructions, skills, hooks, and MCP servers.
- Added legacy documentation archive under docs/legacy.
- Added operational TODO tracking in TODO.md.

## Repository Structure

- docs: project, architecture, and legacy docs.
- scripts: bootstrap and workspace management scripts.
- src: MAUI app source and architecture documentation.
- tests: test structure and guidance.
- workspace-config: active agent packs, instructions, skills, hooks, and MCP server assets.

## Getting Started

Windows:

1. Install prerequisites and tooling as needed.
2. Launch the workspace manager:
   - boot.cmd
3. Open the solution folder in VS Code.

## Current Status

This repository is now aligned to a v0.1 baseline focused on local workspace orchestration and pack-driven setup.

## Next Milestones

- Structured pack.manifest.json support and validation.
- Requirement merge logic across selected packs.
- Activation hardening and test coverage.
