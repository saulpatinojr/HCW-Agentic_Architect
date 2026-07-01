using System.Text.Json;
using WorkspaceManager;
using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class HarnessPipelineServiceTests : IDisposable
{
    private readonly string _repoRoot;
    private readonly string _settingsDir;

    public HarnessPipelineServiceTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "hcw-harness-pipeline-tests", Guid.NewGuid().ToString("N"));
        _settingsDir = Path.Combine(_repoRoot, "settings");
        Directory.CreateDirectory(_repoRoot);
        Directory.CreateDirectory(_settingsDir);
    }

    [Fact]
    public async Task GenerateAsync_WritesOutputsForAllAdapters()
    {
        CreateCanonicalPack();

        var service = CreateService();
        var report = await service.GenerateAsync(_repoRoot, writeOutputs: true);

        Assert.True(report.Succeeded);
        Assert.Equal(3, report.Results.Count);
        Assert.All(report.Results, result => Assert.True(File.Exists(result.OutputPath)));
        Assert.Contains(report.CompatibilityMatrix, entry => entry.AdapterName == "Claude" && entry.Status is "Aligned" or "Generated");
        Assert.Contains(report.CompatibilityMatrix, entry => entry.AdapterName == "Copilot" && entry.Status is "Aligned" or "Generated");
        Assert.Contains(report.CompatibilityMatrix, entry => entry.AdapterName == "Codex" && entry.Status is "Aligned" or "Generated");
    }

    [Fact]
    public async Task ValidateAsync_FlagsDriftWhenGeneratedOutputChanges()
    {
        CreateCanonicalPack();

        var service = CreateService();
        await service.GenerateAsync(_repoRoot, writeOutputs: true);

        string driftPath = Path.Combine(_repoRoot, "workspace-config", "generated", "claude", "ai-architect-agents", "CLAUDE.md");
        await File.AppendAllTextAsync(driftPath, Environment.NewLine + "Manual drift.");

        var validation = await service.ValidateAsync(_repoRoot);

        Assert.False(validation.Succeeded);
        Assert.True(validation.HasDrift);
        Assert.Contains(validation.DriftEntries, entry => entry.Path == driftPath);
        Assert.Contains(validation.CompatibilityMatrix, entry => entry.AdapterName == "Claude" && entry.Status == "Drift");
    }

    [Fact]
    public async Task GenerateAsync_ContinuesWhenOneAdapterFails()
    {
        CreateCanonicalPack();

        var failingAdapter = new ThrowingHarnessAdapter();
        var adapters = new IHarnessAdapter[]
        {
            new ClaudeHarnessAdapter(),
            failingAdapter,
            new CopilotHarnessAdapter(),
            new CodexHarnessAdapter()
        };
        var service = new HarnessPipelineService(new HarnessPolicyService(_settingsDir), adapters);

        var report = await service.GenerateAsync(_repoRoot, writeOutputs: true);

        Assert.False(report.Succeeded);
        Assert.Contains(report.Results, result => !result.Success && result.AdapterName == "Broken");
        Assert.Contains(report.Results, result => result.Success && result.AdapterName == "Claude");
        Assert.Contains(report.Results, result => result.Success && result.AdapterName == "Copilot");
        Assert.Contains(report.Results, result => result.Success && result.AdapterName == "Codex");
        Assert.True(File.Exists(Path.Combine(_repoRoot, "workspace-config", "generated", "claude", "ai-architect-agents", "CLAUDE.md")));
        Assert.True(File.Exists(Path.Combine(_repoRoot, "workspace-config", "generated", "copilot", "ai-architect-agents", "copilot-instructions.md")));
        Assert.True(File.Exists(Path.Combine(_repoRoot, "workspace-config", "generated", "codex", "ai-architect-agents", "instructions.md")));
    }

    private HarnessPipelineService CreateService()
    {
        return new HarnessPipelineService(new HarnessPolicyService(_settingsDir));
    }

    private void CreateCanonicalPack()
    {
        string packDir = Path.Combine(_repoRoot, "workspace-config", "packs", "ai-architect-agents");
        Directory.CreateDirectory(packDir);

        var pack = new CanonicalPackDefinition
        {
            Id = "ai-architect-agents",
            DisplayName = "AI Architect Agents",
            Summary = "Canonical workspace pack used to generate harness-native guidance and policy-aware outputs.",
            Repository = "saulpatinojr/HCW-Agentic_Architect",
            Branch = "main",
            SourcePath = "workspace-config/agents",
            Responsibilities =
            [
                "Coordinate canonical pack generation.",
                "Render harness-native instructions.",
                "Track drift across generated outputs."
            ],
            Handoffs =
            [
                new CanonicalPackHandoff { Label = "Adapter work", Agent = "adapter-team" }
            ],
            Guardrails =
            [
                "Keep generated outputs deterministic.",
                "Regenerate drift before release."
            ],
            Channels =
            [
                "markdown",
                "chat",
                "cli"
            ],
            PolicyEntries =
            [
                new HarnessPolicyEntry { Name = "generate-claude", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "canonical-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generate-copilot", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "canonical-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generate-codex", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "canonical-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generation-mode", Value = "preview", Scope = HarnessPolicyScope.Session, Source = "canonical-pack", Enabled = true }
            ]
        };

        File.WriteAllText(Path.Combine(packDir, "pack.canonical.json"), JsonSerializer.Serialize(pack, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
        {
            Directory.Delete(_repoRoot, true);
        }
    }

    private sealed class ThrowingHarnessAdapter : IHarnessAdapter
    {
        public HarnessAdapterDefinition Definition { get; } = new()
        {
            Name = "broken",
            DisplayName = "Broken",
            OutputFolderName = "broken",
            PrimaryFileName = "broken.md"
        };

        public string GetOutputPath(string repoRootPath, CanonicalPackDefinition pack)
            => Path.Combine(repoRootPath, "workspace-config", "generated", Definition.OutputFolderName, pack.Id, Definition.PrimaryFileName);

        public string BuildContent(CanonicalPackDefinition pack, HarnessPolicyResolution policy)
        {
            throw new InvalidOperationException("Adapter failure for test coverage.");
        }
    }
}
