# HCWMauiApp.Tests

Focused unit tests for services that are independent from MAUI UI/runtime dependencies.

## Current coverage

- `PackManifestService` parsing and validation rules.
- `ManifestRequirementsMergeService` conflict handling and merge behavior.
- `WorkspaceMcpConfigBuilderService` MCP config construction and tokenomics conflict behavior.
- `ToolInstallService` tool mapping and installer probing behavior via fakes.
- `WorkspaceWriterService` dry-run and write-mode sync behavior via fake filesystem.
- `WorkspaceActivationService` integration-style orchestration over a temp workspace.

## Run tests

```powershell
dotnet test src/HCWMauiApp.Tests/HCWMauiApp.Tests.csproj
```

Current expected result: all tests pass. One existing `xUnit2031` analyzer warning remains in `ToolInstallServiceTests.cs`.
