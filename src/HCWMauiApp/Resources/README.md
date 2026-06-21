# Resources

App-wide assets for AI Architect Agents.

## Structure

```text
Resources/
├── Images/    # App logo and provider icons
├── Styles/    # Shared MAUI styles when extracted from page resources
└── Fonts/     # Local fonts, if added later
```

## Image Packaging

`WorkspaceManager.csproj` explicitly includes `Resources\Images\*.*` as `MauiImage` resources. This is required so provider icons are emitted into the Windows build output.

On Windows, MAUI converts both SVG and PNG sources into scaled PNG resources. Runtime references should use `.png` names in XAML and bound model values, for example:

- `ai_architect_agents.png`
- `provider_azure.png`
- `provider_aws.png`
- `provider_vmware.png`

## Provider Asset Rules

- Keep provider images local; do not live-load remote logos.
- Use descriptive lowercase names: `provider_<id>.<ext>`.
- Register providers in `Services/ProviderRegistryService.cs`.
- Do not register ISO as a provider.
- If a source SVG fails to render after packaging, replace it with a local PNG and keep the same logical provider id.

## Current Provider Groups

- Cloud Providers: AWS, Azure, Google Cloud, VMware.
- Service Providers: Ansible, Docker, FinOps Foundation, GitHub, Kubernetes, Terraform.
- AI Providers: Claude, Codex, GitHub Copilot.
