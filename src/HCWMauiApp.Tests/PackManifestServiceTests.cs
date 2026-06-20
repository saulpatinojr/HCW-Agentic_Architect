using WorkspaceManager.Services;

namespace HCWMauiApp.Tests;

public sealed class PackManifestServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly PackManifestService _service = new();

    public PackManifestServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "hcw-manifest-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void ValidatePack_WhenManifestIsMissing_ReturnsWarningAndNoErrors()
    {
        string packPath = CreatePackDirectory("pack-no-manifest");

        var result = _service.ValidatePack(packPath);

        Assert.False(result.HasManifest);
        Assert.True(result.IsValid);
        Assert.Single(result.Warnings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidatePack_WhenJsonIsInvalid_ReturnsError()
    {
        string packPath = CreatePackDirectory("pack-invalid-json");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), "{ this-is-not-json }");

        var result = _service.ValidatePack(packPath);

        Assert.True(result.HasManifest);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.StartsWith("Invalid JSON in pack.manifest.json:"));
    }

    [Fact]
    public void ValidatePack_WhenSchemaAndDisplayNameMissing_ReturnsValidationErrors()
    {
        string packPath = CreatePackDirectory("pack-schema-and-name");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), """
{
  "schemaVersion": 0,
  "displayName": ""
}
""");

        var result = _service.ValidatePack(packPath);

        Assert.False(result.IsValid);
        Assert.Contains("schemaVersion must be greater than 0.", result.Errors);
        Assert.Contains("displayName is required.", result.Errors);
    }

    [Fact]
    public void ValidatePack_WhenEnvVarIsInvalid_ReturnsValidationError()
    {
        string packPath = CreatePackDirectory("pack-invalid-env");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), """
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "requiredEnvVars": ["1INVALID"]
}
""");

        var result = _service.ValidatePack(packPath);

        Assert.False(result.IsValid);
        Assert.Contains("Invalid environment variable name in requiredEnvVars: 1INVALID", result.Errors);
    }

    [Fact]
    public void ValidatePack_WhenDuplicateToolsAndMcpServersPresent_ReturnsValidationErrors()
    {
        string packPath = CreatePackDirectory("pack-duplicate-tools-and-mcp");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), """
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "requiredTools": [
    { "displayName": "Terraform", "command": "terraform", "wingetId": "Hashicorp.Terraform" },
    { "displayName": "Terraform Again", "command": "terraform", "wingetId": "Hashicorp.Terraform" }
  ],
  "requiredMcpServers": [
    { "name": "local-cloud-query", "command": "python", "args": ["server.py"] },
    { "name": "local-cloud-query", "command": "python", "args": ["server.py"] }
  ]
}
""");

        var result = _service.ValidatePack(packPath);

        Assert.False(result.IsValid);
        Assert.Contains("Duplicate tool command detected in requiredTools: terraform", result.Errors);
        Assert.Contains("Duplicate MCP server name detected: local-cloud-query", result.Errors);
    }

    [Fact]
    public void ValidatePack_WhenManifestIsValid_ReturnsParsedManifest()
    {
        string packPath = CreatePackDirectory("pack-valid");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), """
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "requiredTools": [
    { "displayName": "Terraform", "command": "terraform", "wingetId": "Hashicorp.Terraform" }
  ],
  "requiredFiles": ["AGENTS.md"],
  "requiredMcpServers": [
    { "name": "local-cloud-query", "command": "python", "args": ["workspace-config/mcp-servers/local-cloud-query/server.py"] }
  ],
  "requiredEnvVars": ["AZURE_SUBSCRIPTION_ID"]
}
""");

        var result = _service.ValidatePack(packPath);

        Assert.True(result.HasManifest);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Manifest);
        Assert.Equal("Terraform Engineer", result.Manifest!.DisplayName);
        Assert.Single(result.Manifest.RequiredTools);
        Assert.Single(result.Manifest.RequiredFiles);
        Assert.Single(result.Manifest.RequiredMcpServers);
        Assert.Single(result.Manifest.RequiredEnvVars);
    }

    [Fact]
    public void ValidatePack_WhenManifestHasUiMetadata_ReturnsOptionalFields()
    {
        string packPath = CreatePackDirectory("pack-metadata");
        File.WriteAllText(Path.Combine(packPath, "pack.manifest.json"), """
{
  "schemaVersion": 1,
  "displayName": "Terraform Engineer",
  "version": "1.2.3",
  "description": "Infrastructure workspace pack.",
  "category": "Infrastructure",
  "providerIds": ["terraform", "azure"],
  "iconKey": "terraform",
  "officialLinks": [{ "label": "Terraform Docs", "url": "https://developer.hashicorp.com/terraform" }],
  "bestPracticeLinks": [{ "label": "Style Guide", "url": "https://developer.hashicorp.com/terraform/language/style" }],
  "sourceRepository": "example/repo",
  "sourcePath": "workspace-config/agents/tf-engineer"
}
""");

        var result = _service.ValidatePack(packPath);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Manifest);
        Assert.Equal("1.2.3", result.Manifest!.Version);
        Assert.Equal("Infrastructure", result.Manifest.Category);
        Assert.Equal(["terraform", "azure"], result.Manifest.ProviderIds);
        Assert.Single(result.Manifest.OfficialLinks);
        Assert.Single(result.Manifest.BestPracticeLinks);
        Assert.Equal("example/repo", result.Manifest.SourceRepository);
    }

    private string CreatePackDirectory(string name)
    {
        string path = Path.Combine(_tempRoot, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }
}
