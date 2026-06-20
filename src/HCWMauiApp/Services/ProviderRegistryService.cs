namespace WorkspaceManager.Services;

public sealed class ProviderRegistryService
{
    private static readonly IReadOnlyDictionary<string, ProviderInfo> Providers =
        new Dictionary<string, ProviderInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["azure"] = new() { Id = "azure", Name = "Azure", IconAsset = "provider_azure.svg", AccentColor = "#38BDF8", OfficialUrl = "https://azure.microsoft.com/" },
            ["aws"] = new() { Id = "aws", Name = "AWS", IconAsset = "provider_aws.svg", AccentColor = "#FBBF24", OfficialUrl = "https://aws.amazon.com/" },
            ["gcp"] = new() { Id = "gcp", Name = "Google Cloud", IconAsset = "provider_gcp.svg", AccentColor = "#60A5FA", OfficialUrl = "https://cloud.google.com/" },
            ["terraform"] = new() { Id = "terraform", Name = "Terraform", IconAsset = "provider_terraform.svg", AccentColor = "#8B5CF6", OfficialUrl = "https://developer.hashicorp.com/terraform" },
            ["kubernetes"] = new() { Id = "kubernetes", Name = "Kubernetes", IconAsset = "provider_kubernetes.svg", AccentColor = "#38BDF8", OfficialUrl = "https://kubernetes.io/" },
            ["docker"] = new() { Id = "docker", Name = "Docker", IconAsset = "provider_docker.svg", AccentColor = "#22D3EE", OfficialUrl = "https://www.docker.com/" },
            ["github"] = new() { Id = "github", Name = "GitHub", IconAsset = "provider_github.svg", AccentColor = "#E2E8F0", OfficialUrl = "https://github.com/" },
            ["claude"] = new() { Id = "claude", Name = "Claude", IconAsset = "provider_claude.svg", AccentColor = "#F59E0B", OfficialUrl = "https://www.anthropic.com/claude" },
            ["codex"] = new() { Id = "codex", Name = "Codex", IconAsset = "provider_codex.svg", AccentColor = "#67E8F9", OfficialUrl = "https://openai.com/codex" },
            ["copilot"] = new() { Id = "copilot", Name = "GitHub Copilot", IconAsset = "provider_copilot.svg", AccentColor = "#A7F3D0", OfficialUrl = "https://github.com/features/copilot" },
            ["ansible"] = new() { Id = "ansible", Name = "Ansible", IconAsset = "provider_ansible.svg", AccentColor = "#F87171", OfficialUrl = "https://www.ansible.com/" },
            ["vmware"] = new() { Id = "vmware", Name = "VMware", IconAsset = "provider_vmware.svg", AccentColor = "#93C5FD", OfficialUrl = "https://www.vmware.com/" }
        };

    public IReadOnlyList<ProviderInfo> GetAll() => Providers.Values.OrderBy(p => p.Name).ToList();

    public IReadOnlyList<ProviderInfo> Resolve(IEnumerable<string> providerIds)
    {
        return providerIds
            .Select(id => Providers.TryGetValue(id, out var provider)
                ? provider
                : new ProviderInfo { Id = id, Name = id, IconAsset = "provider_generic.svg", OfficialUrl = string.Empty })
            .ToList();
    }
}
