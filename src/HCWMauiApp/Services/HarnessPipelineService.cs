using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class HarnessPipelineService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly HarnessPolicyService _policyService;
    private readonly IReadOnlyList<IHarnessAdapter> _adapters;

    public HarnessPipelineService()
        : this(new HarnessPolicyService())
    {
    }

    public HarnessPipelineService(HarnessPolicyService policyService, IEnumerable<IHarnessAdapter>? adapters = null)
    {
        _policyService = policyService;
        _adapters = (adapters ?? new IHarnessAdapter[]
        {
            new ClaudeHarnessAdapter(),
            new CopilotHarnessAdapter(),
            new CodexHarnessAdapter()
        }).ToList();
    }

    public IReadOnlyList<CanonicalPackDefinition> DiscoverCanonicalPacks(string repoRootPath)
    {
        string packsRoot = Path.Combine(repoRootPath, "workspace-config", "packs");
        if (!Directory.Exists(packsRoot))
        {
            return [CreateFallbackPack()];
        }

        var packs = new List<CanonicalPackDefinition>();
        foreach (var filePath in Directory.EnumerateFiles(packsRoot, "pack.canonical.json", SearchOption.AllDirectories))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var pack = JsonSerializer.Deserialize<CanonicalPackDefinition>(json, JsonOptions);
                if (pack is null || string.IsNullOrWhiteSpace(pack.Id))
                {
                    continue;
                }

                NormalizePack(pack, filePath);
                packs.Add(pack);
            }
            catch
            {
            }
        }

        return packs.Count > 0 ? packs : [CreateFallbackPack()];
    }

    public Task<HarnessGenerationReport> GenerateAsync(
        string repoRootPath,
        bool writeOutputs = true,
        string? packId = null,
        IReadOnlyList<HarnessPolicyEntry>? sessionOverrides = null)
    {
        return Task.FromResult(RunAsync(repoRootPath, writeOutputs, packId, sessionOverrides));
    }

    public Task<HarnessGenerationReport> ValidateAsync(
        string repoRootPath,
        string? packId = null,
        IReadOnlyList<HarnessPolicyEntry>? sessionOverrides = null)
    {
        return Task.FromResult(RunAsync(repoRootPath, false, packId, sessionOverrides));
    }

    private HarnessGenerationReport RunAsync(
        string repoRootPath,
        bool writeOutputs,
        string? packId,
        IReadOnlyList<HarnessPolicyEntry>? sessionOverrides)
    {
        var logs = new List<string>();
        var packs = DiscoverCanonicalPacks(repoRootPath);
        if (!string.IsNullOrWhiteSpace(packId))
        {
            packs = packs.Where(pack => pack.Id.Equals(packId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (packs.Count == 0)
        {
            packs = [CreateFallbackPack()];
        }

        var results = new List<HarnessGenerationResult>();
        var compatibility = new List<HarnessCompatibilityEntry>();
        var driftEntries = new List<HarnessDriftEntry>();
        var effectivePolicy = new List<HarnessPolicyEntry>();
        bool hasFailures = false;
        bool hasDrift = false;

        foreach (var pack in packs)
        {
            var resolution = _policyService.Resolve(pack, sessionOverrides: sessionOverrides);
            effectivePolicy = resolution.EffectiveControls.ToList();
            logs.Add($"[*] Canonical pack: {pack.DisplayName} ({pack.Id})");
            logs.Add($"[*] Policy resolution: {resolution.Summary}");

            foreach (var adapter in _adapters)
            {
                if (!IsAdapterEnabled(adapter, resolution.EffectiveControls))
                {
                    var skippedPath = adapter.GetOutputPath(repoRootPath, pack);
                    results.Add(new HarnessGenerationResult
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = skippedPath,
                        Success = true,
                        WasWritten = false,
                        WasDrifted = false,
                        Message = "Skipped by policy."
                    });

                    compatibility.Add(new HarnessCompatibilityEntry
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = skippedPath,
                        Status = "Skipped",
                        Detail = "Disabled by policy.",
                        IsAvailable = true,
                        IsDrifted = false
                    });
                    continue;
                }

                try
                {
                    string outputPath = adapter.GetOutputPath(repoRootPath, pack);
                    string generatedContent = adapter.BuildContent(pack, resolution);
                    var comparison = CompareOrWrite(outputPath, generatedContent, writeOutputs);
                    var result = new HarnessGenerationResult
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = outputPath,
                        Success = true,
                        WasWritten = comparison.WasWritten,
                        WasDrifted = comparison.WasDrifted,
                        Message = comparison.WasDrifted
                            ? (writeOutputs ? "Generated with drift resolved." : "Drift detected.")
                            : "Already aligned."
                    };

                    results.Add(result);
                    compatibility.Add(new HarnessCompatibilityEntry
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = outputPath,
                        Status = comparison.WasDrifted ? (writeOutputs ? "Generated" : "Drift") : "Aligned",
                        Detail = comparison.WasDrifted
                            ? (writeOutputs ? "Output was regenerated to match the canonical pack." : "Generated content differs from the checked-in output.")
                            : "Generated output already matches the canonical pack.",
                        IsAvailable = true,
                        IsDrifted = comparison.WasDrifted
                    });

                    if (comparison.WasDrifted)
                    {
                        hasDrift = true;
                        driftEntries.Add(new HarnessDriftEntry
                        {
                            Path = outputPath,
                            Status = writeOutputs ? "Regenerated" : "Drift",
                            ExpectedHash = comparison.ExpectedHash,
                            ActualHash = comparison.ActualHash
                        });
                    }

                    logs.Add(writeOutputs
                        ? $"[+] {adapter.Definition.DisplayName}: {(comparison.WasDrifted ? "regenerated" : "already aligned")} -> {outputPath}"
                        : $"[=] {adapter.Definition.DisplayName}: {(comparison.WasDrifted ? "drift detected" : "aligned")} -> {outputPath}");
                }
                catch (Exception ex)
                {
                    hasFailures = true;
                    var failedPath = adapter.GetOutputPath(repoRootPath, pack);
                    results.Add(new HarnessGenerationResult
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = failedPath,
                        Success = false,
                        WasWritten = false,
                        WasDrifted = false,
                        Message = ex.Message,
                        Error = ex.ToString()
                    });
                    compatibility.Add(new HarnessCompatibilityEntry
                    {
                        PackId = pack.Id,
                        AdapterName = adapter.Definition.DisplayName,
                        OutputPath = failedPath,
                        Status = "Failed",
                        Detail = ex.Message,
                        IsAvailable = false,
                        IsDrifted = false
                    });
                    logs.Add($"[-] {adapter.Definition.DisplayName}: {ex.Message}");
                }
            }
        }

        bool succeeded = writeOutputs
            ? !hasFailures
            : !hasFailures && !hasDrift;

        return new HarnessGenerationReport
        {
            Packs = packs,
            Results = results,
            CompatibilityMatrix = compatibility,
            DriftEntries = driftEntries,
            EffectivePolicy = effectivePolicy,
            Logs = logs,
            HasFailures = hasFailures,
            HasDrift = hasDrift,
            Succeeded = succeeded,
            Summary = BuildSummary(packs, results, hasFailures, hasDrift, writeOutputs)
        };
    }

    private static bool IsAdapterEnabled(IHarnessAdapter adapter, IReadOnlyList<HarnessPolicyEntry> controls)
    {
        string controlName = $"generate-{adapter.Definition.Name}";
        var control = controls.LastOrDefault(entry => entry.Name.Equals(controlName, StringComparison.OrdinalIgnoreCase));
        return control?.Enabled is not false && !string.Equals(control?.Value, "disabled", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSummary(IReadOnlyCollection<CanonicalPackDefinition> packs, IReadOnlyCollection<HarnessGenerationResult> results, bool hasFailures, bool hasDrift, bool writeOutputs)
    {
        int adapterCount = results.Count;
        int packCount = packs.Count;
        if (writeOutputs)
        {
            return $"generated={adapterCount} packs={packCount} failures={(hasFailures ? "yes" : "no")} drift={(hasDrift ? "resolved" : "none")}";
        }

        return $"validated={adapterCount} packs={packCount} failures={(hasFailures ? "yes" : "no")} drift={(hasDrift ? "yes" : "no")}";
    }

    private static (bool WasWritten, bool WasDrifted, string ExpectedHash, string ActualHash) CompareOrWrite(string outputPath, string content, bool writeOutputs)
    {
        string directory = Path.GetDirectoryName(outputPath)!;
        Directory.CreateDirectory(directory);

        string normalized = NormalizeContent(content);
        string current = File.Exists(outputPath) ? NormalizeContent(File.ReadAllText(outputPath)) : string.Empty;
        string expectedHash = ComputeHash(normalized);
        string actualHash = File.Exists(outputPath) ? ComputeHash(current) : string.Empty;
        bool wasDrifted = !File.Exists(outputPath) || !string.Equals(current, normalized, StringComparison.Ordinal);

        if (writeOutputs && wasDrifted)
        {
            File.WriteAllText(outputPath, content);
            actualHash = expectedHash;
            return (true, true, expectedHash, actualHash);
        }

        return (false, wasDrifted, expectedHash, actualHash);
    }

    private static string NormalizeContent(string content)
    {
        return content.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd() + "\n";
    }

    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
    }

    private static CanonicalPackDefinition CreateFallbackPack()
    {
        return new CanonicalPackDefinition
        {
            Id = "ai-architect-agents",
            DisplayName = "AI Architect Agents",
            Summary = "Fallback canonical pack used when the repo does not yet contain generated pack sources.",
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
            Channels = ["markdown", "chat", "cli"],
            PolicyEntries =
            [
                new HarnessPolicyEntry { Name = "generate-claude", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "fallback-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generate-copilot", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "fallback-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generate-codex", Value = "enabled", Scope = HarnessPolicyScope.Pack, Source = "fallback-pack", Enabled = true },
                new HarnessPolicyEntry { Name = "generation-mode", Value = "preview", Scope = HarnessPolicyScope.Session, Source = "fallback-pack", Enabled = true }
            ]
        };
    }

    private static void NormalizePack(CanonicalPackDefinition pack, string filePath)
    {
        pack.DisplayName = string.IsNullOrWhiteSpace(pack.DisplayName) ? pack.Id : pack.DisplayName;
        pack.Summary = string.IsNullOrWhiteSpace(pack.Summary) ? $"Canonical pack loaded from {Path.GetFileName(filePath)}." : pack.Summary;
        pack.Repository = string.IsNullOrWhiteSpace(pack.Repository) ? "saulpatinojr/HCW-Agentic_Architect" : pack.Repository;
        pack.Branch = string.IsNullOrWhiteSpace(pack.Branch) ? "main" : pack.Branch;
        pack.SourcePath = string.IsNullOrWhiteSpace(pack.SourcePath) ? Path.GetDirectoryName(filePath) ?? string.Empty : pack.SourcePath;

        pack.PolicyEntries ??= [];
        pack.Responsibilities ??= [];
        pack.Handoffs ??= [];
        pack.Guardrails ??= [];
        pack.Channels ??= [];
    }
}

public interface IHarnessAdapter
{
    HarnessAdapterDefinition Definition { get; }
    string GetOutputPath(string repoRootPath, CanonicalPackDefinition pack);
    string BuildContent(CanonicalPackDefinition pack, HarnessPolicyResolution policy);
}

public sealed class ClaudeHarnessAdapter : IHarnessAdapter
{
    public HarnessAdapterDefinition Definition { get; } = new()
    {
        Name = "claude",
        DisplayName = "Claude",
        OutputFolderName = "claude",
        PrimaryFileName = "CLAUDE.md"
    };

    public string GetOutputPath(string repoRootPath, CanonicalPackDefinition pack)
        => Path.Combine(repoRootPath, "workspace-config", "generated", Definition.OutputFolderName, pack.Id, Definition.PrimaryFileName);

    public string BuildContent(CanonicalPackDefinition pack, HarnessPolicyResolution policy)
    {
        return HarnessAdapterMarkdownBuilder.BuildMarkdown(pack, policy, "Claude");
    }
}

public sealed class CopilotHarnessAdapter : IHarnessAdapter
{
    public HarnessAdapterDefinition Definition { get; } = new()
    {
        Name = "copilot",
        DisplayName = "Copilot",
        OutputFolderName = "copilot",
        PrimaryFileName = "copilot-instructions.md"
    };

    public string GetOutputPath(string repoRootPath, CanonicalPackDefinition pack)
        => Path.Combine(repoRootPath, "workspace-config", "generated", Definition.OutputFolderName, pack.Id, Definition.PrimaryFileName);

    public string BuildContent(CanonicalPackDefinition pack, HarnessPolicyResolution policy)
    {
        return HarnessAdapterMarkdownBuilder.BuildMarkdown(pack, policy, "Copilot");
    }
}

public sealed class CodexHarnessAdapter : IHarnessAdapter
{
    public HarnessAdapterDefinition Definition { get; } = new()
    {
        Name = "codex",
        DisplayName = "Codex",
        OutputFolderName = "codex",
        PrimaryFileName = "instructions.md"
    };

    public string GetOutputPath(string repoRootPath, CanonicalPackDefinition pack)
        => Path.Combine(repoRootPath, "workspace-config", "generated", Definition.OutputFolderName, pack.Id, Definition.PrimaryFileName);

    public string BuildContent(CanonicalPackDefinition pack, HarnessPolicyResolution policy)
    {
        return HarnessAdapterMarkdownBuilder.BuildMarkdown(pack, policy, "Codex");
    }
}

internal static class HarnessAdapterMarkdownBuilder
{
    public static string BuildMarkdown(CanonicalPackDefinition pack, HarnessPolicyResolution policy, string harnessName)
    {
        var lines = new List<string>
        {
            $"# {pack.DisplayName} ({harnessName})",
            string.Empty,
            pack.Summary,
            string.Empty,
            "## Responsibilities"
        };

        if (pack.Responsibilities.Count == 0)
        {
            lines.Add("- None declared.");
        }
        else
        {
            lines.AddRange(pack.Responsibilities.Select(item => $"- {item}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Handoffs");
        if (pack.Handoffs.Count == 0)
        {
            lines.Add("- None declared.");
        }
        else
        {
            lines.AddRange(pack.Handoffs.Select(item => $"- {item.Label}: {item.Agent}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Guardrails");
        if (pack.Guardrails.Count == 0)
        {
            lines.Add("- None declared.");
        }
        else
        {
            lines.AddRange(pack.Guardrails.Select(item => $"- {item}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Channels");
        if (pack.Channels.Count == 0)
        {
            lines.Add("- None declared.");
        }
        else
        {
            lines.AddRange(pack.Channels.Select(item => $"- {item}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Effective Policy");
        foreach (var control in policy.EffectiveControls)
        {
            lines.Add($"- {control.Name} = {control.Value} [{control.Scope}] from {control.Source} (precedence {control.Precedence})");
        }

        lines.Add(string.Empty);
        lines.Add("## Source");
        lines.Add($"- repository: {pack.Repository}");
        lines.Add($"- branch: {pack.Branch}");
        lines.Add($"- path: {pack.SourcePath}");

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}

