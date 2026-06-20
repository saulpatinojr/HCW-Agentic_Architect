namespace WorkspaceManager.Services;

public sealed class WorkspaceFolderService
{
    public IReadOnlyList<WorkspaceFolderNode> BuildNodes(string repoRootPath, AgentViewModel? selectedPack)
    {
        var nodes = new List<WorkspaceFolderNode>
        {
            Create("workspace-config", Path.Combine(repoRootPath, "workspace-config"), "Folder"),
            Create("agents", Path.Combine(repoRootPath, "workspace-config", "agents"), "Folder"),
            Create("mcp-servers", Path.Combine(repoRootPath, "workspace-config", "mcp-servers"), "Folder"),
            Create(".vscode/mcp.json", Path.Combine(repoRootPath, ".vscode", "mcp.json"), "File"),
            Create("AGENTS.md", Path.Combine(repoRootPath, "AGENTS.md"), "File"),
            Create("CLAUDE.md", Path.Combine(repoRootPath, "CLAUDE.md"), "File"),
            Create("copilot-instructions.md", Path.Combine(repoRootPath, ".github", "copilot-instructions.md"), "File")
        };

        if (selectedPack is not null)
        {
            nodes.Insert(2, Create(selectedPack.DirectoryName, selectedPack.FullPath, "Pack"));
        }

        return nodes;
    }

    private static WorkspaceFolderNode Create(string label, string path, string kind)
    {
        return new WorkspaceFolderNode
        {
            Label = label,
            Path = path,
            Kind = kind,
            Exists = Directory.Exists(path) || File.Exists(path)
        };
    }
}
