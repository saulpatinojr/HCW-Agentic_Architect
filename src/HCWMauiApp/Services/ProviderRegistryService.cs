namespace WorkspaceManager.Services;

public sealed class ProviderRegistryService
{
    private static readonly IReadOnlyDictionary<string, ProviderInfo> Providers =
        new Dictionary<string, ProviderInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["azure"] = new() { Id = "azure", Name = "Azure", Group = "Cloud Providers", IconAsset = "provider_azure.png", AccentColor = "#0078D4", OfficialUrl = "https://azure.microsoft.com/" },
            ["aws"] = new() { Id = "aws", Name = "AWS", Group = "Cloud Providers", IconAsset = "provider_aws.png", AccentColor = "#FFB900", OfficialUrl = "https://aws.amazon.com/" },
            ["gcp"] = new() { Id = "gcp", Name = "Google Cloud", Group = "Cloud Providers", IconAsset = "provider_gcp.png", AccentColor = "#4285F4", OfficialUrl = "https://cloud.google.com/" },
            ["vmware"] = new() { Id = "vmware", Name = "VMware", Group = "Cloud Providers", IconAsset = "provider_vmware.png", AccentColor = "#607D8B", OfficialUrl = "https://www.vmware.com/" },
            ["terraform"] = new() { Id = "terraform", Name = "Terraform", Group = "Service Providers", IconAsset = "provider_terraform.png", AccentColor = "#7B42BC", OfficialUrl = "https://developer.hashicorp.com/terraform" },
            ["finops"] = new() { Id = "finops", Name = "FinOps Foundation", Group = "Service Providers", IconAsset = "provider_finops.png", AccentColor = "#04C89A", OfficialUrl = "https://www.finops.org/" },
            ["kubernetes"] = new() { Id = "kubernetes", Name = "Kubernetes", Group = "Service Providers", IconAsset = "provider_kubernetes.png", AccentColor = "#326CE5", OfficialUrl = "https://kubernetes.io/" },
            ["docker"] = new() { Id = "docker", Name = "Docker", Group = "Service Providers", IconAsset = "provider_docker.png", AccentColor = "#2496ED", OfficialUrl = "https://www.docker.com/" },
            ["github"] = new() { Id = "github", Name = "GitHub", Group = "Service Providers", IconAsset = "provider_github.png", AccentColor = "#24292F", OfficialUrl = "https://github.com/" },
            ["ansible"] = new() { Id = "ansible", Name = "Ansible", Group = "Service Providers", IconAsset = "provider_ansible.png", AccentColor = "#EE0000", OfficialUrl = "https://www.ansible.com/" },
            ["claude"] = new() { Id = "claude", Name = "Claude", Group = "AI Providers", IconAsset = "provider_claude.png", AccentColor = "#D97706", OfficialUrl = "https://www.anthropic.com/claude" },
            ["codex"] = new() { Id = "codex", Name = "Codex", Group = "AI Providers", IconAsset = "provider_codex.png", AccentColor = "#0078D4", OfficialUrl = "https://openai.com/codex" },
            ["copilot"] = new() { Id = "copilot", Name = "GitHub Copilot", Group = "AI Providers", IconAsset = "provider_copilot.png", AccentColor = "#107C10", OfficialUrl = "https://github.com/features/copilot" }
        };

    public IReadOnlyList<ProviderInfo> GetAll() => Providers.Values.OrderBy(p => p.Name).ToList();

    public IReadOnlyList<ProviderGroup> GetGroups()
    {
        string[] order = ["Cloud Providers", "Service Providers", "AI Providers"];
        return order
            .Select(group => new ProviderGroup
            {
                Name = group,
                Providers = Providers.Values
                    .Where(provider => provider.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(provider => provider.Name)
                    .ToList()
            })
            .ToList();
    }

    public IReadOnlyList<ProviderInfo> Resolve(IEnumerable<string> providerIds)
    {
        return providerIds
            .Select(id => Providers.TryGetValue(id, out var provider)
                ? provider
                : new ProviderInfo { Id = id, Name = id, Group = "Service Providers", IconAsset = "provider_generic.png", OfficialUrl = string.Empty })
            .ToList();
    }
}
