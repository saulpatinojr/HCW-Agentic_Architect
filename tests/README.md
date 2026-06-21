# Tests

This folder is reserved for future integration and end-to-end test assets.

Current unit tests live in:

```text
src/HCWMauiApp.Tests/
```

## Run Current Tests

```powershell
dotnet test src\HCWMauiApp.Tests\HCWMauiApp.Tests.csproj
```

Current expected result: all tests pass. One existing `xUnit2031` analyzer warning remains in `ToolInstallServiceTests.cs`.

## Current Coverage Focus

- Pack manifest parsing and validation.
- Requirement merge behavior.
- Tool install/preflight mapping.
- Workspace catalog and update-state logic.
- MCP helper verification.

## Future Test Assets

Use this folder for tests that are not tied directly to the MAUI unit-test project, such as:

- Pack fixture repositories.
- End-to-end activation scenarios.
- UI automation notes.
- Manual validation checklists.
