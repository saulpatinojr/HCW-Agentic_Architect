using AgenticWorkspaceManager;
using AgenticWorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class ManifestRequirementsMergeServiceTests
{
    [Fact]
    public void Merge_WhenToolWingetIdsConflict_ReturnsError()
    {
        var service = new ManifestRequirementsMergeService(new FakeEnvironment());

        var results = new[]
        {
            CreateValidation("pack-a", manifest: new PackManifest
            {
                SchemaVersion = 1,
                DisplayName = "Pack A",
                RequiredTools =
                [
                    new ManifestToolRequirement { DisplayName = "Terraform", Command = "terraform", WingetId = "Hashicorp.Terraform" }
                ]
            }),
            CreateValidation("pack-b", manifest: new PackManifest
            {
                SchemaVersion = 1,
                DisplayName = "Pack B",
                RequiredTools =
                [
                    new ManifestToolRequirement { DisplayName = "Terraform", Command = "terraform", WingetId = "Contoso.Terraform" }
                ]
            })
        };

        var merged = service.Merge(results);

        Assert.False(merged.IsValid);
        Assert.Contains("Tool 'terraform' has conflicting winget IDs across selected packs.", merged.Errors);
    }

    [Fact]
    public void Merge_WhenMcpDefinitionsConflict_ReturnsError()
    {
        var service = new ManifestRequirementsMergeService(new FakeEnvironment());

        var results = new[]
        {
            CreateValidation("pack-a", manifest: new PackManifest
            {
                SchemaVersion = 1,
                DisplayName = "Pack A",
                RequiredMcpServers =
                [
                    new ManifestMcpServerRequirement { Name = "local-cloud-query", Command = "python", Args = ["server-a.py"] }
                ]
            }),
            CreateValidation("pack-b", manifest: new PackManifest
            {
                SchemaVersion = 1,
                DisplayName = "Pack B",
                RequiredMcpServers =
                [
                    new ManifestMcpServerRequirement { Name = "local-cloud-query", Command = "python", Args = ["server-b.py"] }
                ]
            })
        };

        var merged = service.Merge(results);

        Assert.False(merged.IsValid);
        Assert.Contains("MCP server 'local-cloud-query' has conflicting definitions across selected packs.", merged.Errors);
    }

    [Fact]
    public void Merge_WhenRequiredFileIsMissing_ReturnsError()
    {
        var environment = new FakeEnvironment();
        var service = new ManifestRequirementsMergeService(environment);

        var result = CreateValidation("pack-a", manifest: new PackManifest
        {
            SchemaVersion = 1,
            DisplayName = "Pack A",
            RequiredFiles = ["AGENTS.md"]
        });

        var merged = service.Merge([result]);

        Assert.False(merged.IsValid);
        Assert.Contains("Pack 'pack-a' is missing required file: AGENTS.md", merged.Errors);
    }

    [Fact]
    public void Merge_WhenEnvVarMissing_ReturnsWarning()
    {
        var service = new ManifestRequirementsMergeService(new FakeEnvironment());

        var result = CreateValidation("pack-a", manifest: new PackManifest
        {
            SchemaVersion = 1,
            DisplayName = "Pack A",
            RequiredEnvVars = ["AZURE_SUBSCRIPTION_ID"]
        });

        var merged = service.Merge([result]);

        Assert.True(merged.IsValid);
        Assert.Contains("Environment variable 'AZURE_SUBSCRIPTION_ID' is required by selected packs but is not set.", merged.Warnings);
    }

    [Fact]
    public void Merge_WhenFileExistsAndEnvIsSet_MergesRequirements()
    {
        var environment = new FakeEnvironment();
        environment.ExistingFiles.Add(Path.Combine("c:\\repo\\pack-a", "AGENTS.md"));
        environment.EnvironmentVariables["AZURE_SUBSCRIPTION_ID"] = "sub-123";
        var service = new ManifestRequirementsMergeService(environment);

        var result = CreateValidation("pack-a", fullPath: "c:\\repo\\pack-a", manifest: new PackManifest
        {
            SchemaVersion = 1,
            DisplayName = "Pack A",
            RequiredTools =
            [
                new ManifestToolRequirement { DisplayName = "Terraform", Command = "terraform", WingetId = "Hashicorp.Terraform" }
            ],
            RequiredFiles = ["AGENTS.md"],
            RequiredEnvVars = ["AZURE_SUBSCRIPTION_ID"]
        });

        var merged = service.Merge([result]);

        Assert.True(merged.IsValid);
        Assert.Empty(merged.Warnings);
        Assert.Single(merged.RequiredTools);
        Assert.Single(merged.RequiredFiles);
        Assert.Single(merged.RequiredEnvVars);
    }

    private static PackValidation CreateValidation(string directoryName, PackManifest manifest, string? fullPath = null)
    {
        fullPath ??= Path.Combine("c:\\repo", directoryName);

        return new PackValidation(
            new AgentViewModel
            {
                DirectoryName = directoryName,
                FriendlyName = directoryName,
                FullPath = fullPath
            },
            new PackManifestValidationResult
            {
                HasManifest = true,
                Manifest = manifest,
                PackPath = fullPath,
                ManifestPath = Path.Combine(fullPath, "pack.manifest.json")
            });
    }

    private sealed class FakeEnvironment : IManifestRequirementsEnvironment
    {
        public HashSet<string> ExistingFiles { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> EnvironmentVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool FileExists(string path)
        {
            return ExistingFiles.Contains(path);
        }

        public string? GetEnvironmentVariable(string name)
        {
            return EnvironmentVariables.TryGetValue(name, out var value) ? value : null;
        }
    }
}
