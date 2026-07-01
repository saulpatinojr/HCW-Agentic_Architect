using System.Text.Json;

namespace WorkspaceManager.Services;

public sealed class HarnessPolicyService
{
    private const string PolicyFileName = "harness-policy.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _settingsDir;

    public HarnessPolicyService(string? settingsDirectory = null)
    {
        _settingsDir = settingsDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIArchitectAgents");
        Directory.CreateDirectory(_settingsDir);
    }

    public HarnessPolicyManifest Load()
    {
        string path = ResolvePolicyPath();
        if (!File.Exists(path))
        {
            return HarnessPolicyManifest.Default();
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<HarnessPolicyManifest>(json, JsonOptions) ?? HarnessPolicyManifest.Default();
        }
        catch
        {
            return HarnessPolicyManifest.Default();
        }
    }

    public async Task SaveAsync(HarnessPolicyManifest manifest)
    {
        string path = ResolvePolicyPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string json = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public HarnessPolicyResolution Resolve(CanonicalPackDefinition pack, HarnessPolicyManifest? overrideManifest = null, IReadOnlyList<HarnessPolicyEntry>? sessionOverrides = null)
    {
        var manifest = overrideManifest ?? Load();
        var allControls = new List<HarnessPolicyEntry>();

        allControls.AddRange(pack.PolicyEntries.Select(CloneAndNormalize(HarnessPolicyScope.Pack, "canonical-pack")));
        allControls.AddRange(manifest.Controls.Select(CloneAndNormalizeFromManifest()));

        if (sessionOverrides is not null)
        {
            allControls.AddRange(sessionOverrides.Select(CloneAndNormalize(HarnessPolicyScope.Session, "session-override")));
        }

        foreach (var control in allControls)
        {
            control.Precedence = GetPrecedence(control.Scope);
        }

        var effectiveControls = allControls
            .Where(control => control.Enabled)
            .GroupBy(control => control.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(control => control.Precedence)
                .ThenBy(control => control.Source, StringComparer.OrdinalIgnoreCase)
                .Last())
            .OrderBy(control => control.Precedence)
            .ThenBy(control => control.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new HarnessPolicyResolution
        {
            Pack = pack,
            AllControls = allControls,
            EffectiveControls = effectiveControls,
            Summary = BuildSummary(manifest, allControls, effectiveControls)
        };
    }

    public IReadOnlyList<HarnessPolicyScope> GetScopes() => Enum.GetValues<HarnessPolicyScope>();

    public string GetDefaultManifestJson()
    {
        return JsonSerializer.Serialize(HarnessPolicyManifest.Default(), JsonOptions);
    }

    private static Func<HarnessPolicyEntry, HarnessPolicyEntry> CloneAndNormalize(HarnessPolicyScope scope, string source)
    {
        return entry => new HarnessPolicyEntry
        {
            Name = entry.Name,
            Value = entry.Value,
            Scope = scope,
            Source = string.IsNullOrWhiteSpace(entry.Source) ? source : entry.Source,
            Enabled = entry.Enabled
        };
    }

    private static Func<HarnessPolicyEntry, HarnessPolicyEntry> CloneAndNormalizeFromManifest()
    {
        return entry => new HarnessPolicyEntry
        {
            Name = entry.Name,
            Value = entry.Value,
            Scope = entry.Scope,
            Source = string.IsNullOrWhiteSpace(entry.Source) ? "local" : entry.Source,
            Enabled = entry.Enabled
        };
    }

    private static int GetPrecedence(HarnessPolicyScope scope)
    {
        return scope switch
        {
            HarnessPolicyScope.Global => 100,
            HarnessPolicyScope.Pack => 200,
            HarnessPolicyScope.Session => 300,
            _ => 0
        };
    }

    private static string BuildSummary(HarnessPolicyManifest manifest, IReadOnlyCollection<HarnessPolicyEntry> allControls, IReadOnlyCollection<HarnessPolicyEntry> effectiveControls)
    {
        int globalCount = allControls.Count(control => control.Scope == HarnessPolicyScope.Global);
        int packCount = allControls.Count(control => control.Scope == HarnessPolicyScope.Pack);
        int sessionCount = allControls.Count(control => control.Scope == HarnessPolicyScope.Session);
        return $"pack={manifest.ActivePackId} | global={globalCount} pack={packCount} session={sessionCount} effective={effectiveControls.Count}";
    }

    private string ResolvePolicyPath()
    {
        return Path.Combine(_settingsDir, PolicyFileName);
    }
}

