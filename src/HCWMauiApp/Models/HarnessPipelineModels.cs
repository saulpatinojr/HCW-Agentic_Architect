namespace WorkspaceManager;

public enum HarnessPolicyScope
{
    Global = 0,
    Pack = 1,
    Session = 2
}

public sealed class CanonicalPackDefinition
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string SourcePath { get; set; } = string.Empty;
    public List<string> Responsibilities { get; set; } = [];
    public List<CanonicalPackHandoff> Handoffs { get; set; } = [];
    public List<string> Guardrails { get; set; } = [];
    public List<string> Channels { get; set; } = [];
    public List<HarnessPolicyEntry> PolicyEntries { get; set; } = [];
}

public sealed class CanonicalPackHandoff
{
    public string Label { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
}

public sealed class HarnessPolicyEntry
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public HarnessPolicyScope Scope { get; set; } = HarnessPolicyScope.Global;
    public string Source { get; set; } = "local";
    public bool Enabled { get; set; } = true;
    public int Precedence { get; set; }
}

public sealed class HarnessPolicyManifest
{
    public string ActivePackId { get; set; } = "ai-architect-agents";
    public List<HarnessPolicyEntry> Controls { get; set; } = [];

    public static HarnessPolicyManifest Default()
    {
        return new HarnessPolicyManifest
        {
            Controls =
            [
                new HarnessPolicyEntry
                {
                    Name = "generate-claude",
                    Value = "enabled",
                    Scope = HarnessPolicyScope.Global,
                    Source = "local-default",
                    Enabled = true
                },
                new HarnessPolicyEntry
                {
                    Name = "generate-copilot",
                    Value = "enabled",
                    Scope = HarnessPolicyScope.Global,
                    Source = "local-default",
                    Enabled = true
                },
                new HarnessPolicyEntry
                {
                    Name = "generate-codex",
                    Value = "enabled",
                    Scope = HarnessPolicyScope.Global,
                    Source = "local-default",
                    Enabled = true
                },
                new HarnessPolicyEntry
                {
                    Name = "generation-mode",
                    Value = "preview",
                    Scope = HarnessPolicyScope.Session,
                    Source = "local-default",
                    Enabled = true
                }
            ]
        };
    }
}

public sealed class HarnessPolicyResolution
{
    public CanonicalPackDefinition Pack { get; set; } = new();
    public IReadOnlyList<HarnessPolicyEntry> AllControls { get; set; } = [];
    public IReadOnlyList<HarnessPolicyEntry> EffectiveControls { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}

public sealed class HarnessAdapterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string OutputFolderName { get; set; } = string.Empty;
    public string PrimaryFileName { get; set; } = string.Empty;
}

public sealed class HarnessGenerationResult
{
    public string PackId { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public bool WasWritten { get; set; }
    public bool WasDrifted { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public sealed class HarnessCompatibilityEntry
{
    public string PackId { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public bool IsDrifted { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class HarnessDriftEntry
{
    public string Path { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ExpectedHash { get; set; } = string.Empty;
    public string ActualHash { get; set; } = string.Empty;
}

public sealed class HarnessGenerationReport
{
    public IReadOnlyList<CanonicalPackDefinition> Packs { get; set; } = [];
    public IReadOnlyList<HarnessGenerationResult> Results { get; set; } = [];
    public IReadOnlyList<HarnessCompatibilityEntry> CompatibilityMatrix { get; set; } = [];
    public IReadOnlyList<HarnessDriftEntry> DriftEntries { get; set; } = [];
    public IReadOnlyList<HarnessPolicyEntry> EffectivePolicy { get; set; } = [];
    public IReadOnlyList<string> Logs { get; set; } = [];
    public bool HasFailures { get; set; }
    public bool HasDrift { get; set; }
    public bool Succeeded { get; set; }
    public string Summary { get; set; } = string.Empty;
}
