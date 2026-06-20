namespace AgenticWorkspaceManager.Services;

public sealed class ManifestRequirementsMergeService
{
    private readonly IManifestRequirementsEnvironment _environment;

    public ManifestRequirementsMergeService()
        : this(new DefaultManifestRequirementsEnvironment())
    {
    }

    public ManifestRequirementsMergeService(IManifestRequirementsEnvironment environment)
    {
        _environment = environment;
    }

    public MergedManifestRequirements Merge(IEnumerable<PackValidation> results)
    {
        var merged = new MergedManifestRequirements();

        foreach (var result in results)
        {
            if (!result.Validation.HasManifest || result.Validation.Manifest is null)
            {
                continue;
            }

            var manifest = result.Validation.Manifest;

            foreach (var tool in manifest.RequiredTools)
            {
                if (!merged.RequiredTools.TryGetValue(tool.Command, out var existingTool))
                {
                    merged.RequiredTools[tool.Command] = new ManifestToolRequirement
                    {
                        DisplayName = tool.DisplayName,
                        Command = tool.Command,
                        WingetId = tool.WingetId
                    };
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(existingTool.WingetId)
                    && !string.IsNullOrWhiteSpace(tool.WingetId)
                    && !existingTool.WingetId.Equals(tool.WingetId, StringComparison.OrdinalIgnoreCase))
                {
                    merged.Errors.Add($"Tool '{tool.Command}' has conflicting winget IDs across selected packs.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(existingTool.WingetId) && !string.IsNullOrWhiteSpace(tool.WingetId))
                {
                    existingTool.WingetId = tool.WingetId;
                }
            }

            foreach (var requiredFile in manifest.RequiredFiles)
            {
                var normalized = requiredFile.Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(result.Agent.FullPath, normalized);
                if (!_environment.FileExists(filePath))
                {
                    merged.Errors.Add($"Pack '{result.Agent.DirectoryName}' is missing required file: {requiredFile}");
                    continue;
                }

                merged.RequiredFiles.Add(filePath);
            }

            foreach (var envVar in manifest.RequiredEnvVars)
            {
                merged.RequiredEnvVars.Add(envVar);
            }

            foreach (var mcp in manifest.RequiredMcpServers)
            {
                if (!merged.RequiredMcpServers.TryGetValue(mcp.Name, out var existingMcp))
                {
                    merged.RequiredMcpServers[mcp.Name] = new ManifestMcpServerRequirement
                    {
                        Name = mcp.Name,
                        Command = mcp.Command,
                        Args = [.. mcp.Args]
                    };
                    continue;
                }

                bool sameCommand = string.Equals(existingMcp.Command, mcp.Command, StringComparison.OrdinalIgnoreCase);
                bool sameArgs = existingMcp.Args.SequenceEqual(mcp.Args, StringComparer.OrdinalIgnoreCase);
                if (!sameCommand || !sameArgs)
                {
                    merged.Errors.Add($"MCP server '{mcp.Name}' has conflicting definitions across selected packs.");
                }
            }
        }

        foreach (var envVar in merged.RequiredEnvVars)
        {
            if (string.IsNullOrWhiteSpace(_environment.GetEnvironmentVariable(envVar)))
            {
                merged.Warnings.Add($"Environment variable '{envVar}' is required by selected packs but is not set.");
            }
        }

        return merged;
    }
}

public interface IManifestRequirementsEnvironment
{
    bool FileExists(string path);
    string? GetEnvironmentVariable(string name);
}

public sealed class DefaultManifestRequirementsEnvironment : IManifestRequirementsEnvironment
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }
}
