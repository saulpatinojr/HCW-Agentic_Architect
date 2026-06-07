#Requires -Version 7.0
<#
.SYNOPSIS
    HCW Agentic Architect — Windows CLI Installer (Primary)
    Installs all AI assistant CLIs, IaC tools, and developer utilities via winget.

.DESCRIPTION
    Platform : Windows x64 / ARM64 (primary)
    Package manager: winget
    Run as: regular user (winget does not require elevation for most packages)
    Elevation: required only for Docker Desktop — script will prompt

.USAGE
    pwsh ./scripts/install_cli.ps1
    pwsh ./scripts/install_cli.ps1 -SkipDocker -SkipAntigravity

.NOTES
    After running this script, run: pwsh ./scripts/configure_repo.ps1
#>

[CmdletBinding()]
param(
    [switch]$SkipDocker,
    [switch]$SkipAntigravity,
    [switch]$SkipAnsible,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step { param([string]$Msg) Write-Host "`n==> $Msg" -ForegroundColor Cyan }
function Write-OK   { param([string]$Msg) Write-Host "  [OK] $Msg" -ForegroundColor Green }
function Write-Skip { param([string]$Msg) Write-Host "  [--] $Msg (skipped)" -ForegroundColor DarkGray }
function Write-Warn { param([string]$Msg) Write-Host "  [!!] $Msg" -ForegroundColor Yellow }

function Install-WingetPackage {
    param([string]$Id, [string]$Name, [switch]$Skip)
    if ($Skip) { Write-Skip $Name; return }
    Write-Host "  Installing $Name ($Id)..." -NoNewline
    if ($DryRun) { Write-Host " [DRY RUN]" -ForegroundColor DarkYellow; return }
    $result = winget install --id $Id --silent --accept-package-agreements --accept-source-agreements 2>&1
    if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq -1978335189) {
        # -1978335189 = APPINSTALLER_ERROR_ALREADY_INSTALLED
        Write-OK $Name
    } else {
        Write-Warn "$Name may have failed (exit $LASTEXITCODE) — check manually"
    }
}

# ─────────────────────────────────────────────
# 0. Verify winget is available
# ─────────────────────────────────────────────
Write-Step "Checking winget availability"
if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
    Write-Error "winget not found. Install 'App Installer' from the Microsoft Store, then re-run."
    exit 1
}
Write-OK "winget found: $(winget --version)"

# ─────────────────────────────────────────────
# 1. Core developer runtime
# ─────────────────────────────────────────────
Write-Step "Core developer runtime"
Install-WingetPackage -Id "Git.Git"                   -Name "Git"
Install-WingetPackage -Id "GitHub.cli"                -Name "GitHub CLI (gh)"
Install-WingetPackage -Id "Microsoft.PowerShell"      -Name "PowerShell 7+"
Install-WingetPackage -Id "OpenJS.NodeJS.LTS"         -Name "Node.js LTS"
Install-WingetPackage -Id "Python.Python.3.12"        -Name "Python 3.12"

# ─────────────────────────────────────────────
# 2. IaC + Azure toolchain
# ─────────────────────────────────────────────
Write-Step "IaC + Azure toolchain"
Install-WingetPackage -Id "Microsoft.AzureCLI"        -Name "Azure CLI (az)"
Install-WingetPackage -Id "Hashicorp.Terraform"       -Name "Terraform"
Install-WingetPackage -Id "Hashicorp.Packer"          -Name "Packer"
Install-WingetPackage -Id "RedHat.Ansible"            -Name "Ansible" -Skip:$SkipAnsible

# Install Bicep via Azure CLI (not a winget package)
Write-Host "  Installing Bicep via az bicep install..." -NoNewline
if (-not $DryRun) {
    az bicep install 2>&1 | Out-Null
    Write-OK "Bicep"
} else {
    Write-Host " [DRY RUN]" -ForegroundColor DarkYellow
}

# ─────────────────────────────────────────────
# 3. IDE + editor CLIs
# ─────────────────────────────────────────────
Write-Step "IDE + editor CLIs"
Install-WingetPackage -Id "Microsoft.VisualStudioCode" -Name "VS Code"
Install-WingetPackage -Id "GitHub.GitHubDesktop"       -Name "GitHub Desktop"

# VS Code CLI is bundled with VS Code — verify it's on PATH
Write-Host "  Checking VS Code CLI (code)..." -NoNewline
if (Get-Command code -ErrorAction SilentlyContinue) {
    Write-OK "VS Code CLI (code) already on PATH"
} else {
    Write-Warn "'code' not on PATH yet — restart terminal after VS Code install or add manually"
}

# Antigravity
if ($SkipAntigravity) {
    Write-Skip "Antigravity"
} else {
    Write-Host "  Checking Antigravity CLI (antigravity)..." -NoNewline
    if (Get-Command antigravity -ErrorAction SilentlyContinue) {
        Write-OK "Antigravity CLI already installed"
    } else {
        Write-Warn "Antigravity not found via winget. Install from https://antigravity.dev then re-run to validate."
    }
}

# ─────────────────────────────────────────────
# 4. AI assistant CLIs
# ─────────────────────────────────────────────
Write-Step "AI assistant CLIs (npm global)"

$npmPackages = @(
    @{ Pkg = "@anthropic-ai/claude-code"; Name = "Claude Code CLI" },
    @{ Pkg = "@openai/codex";             Name = "OpenAI Codex CLI" },
    @{ Pkg = "@google/gemini-cli";        Name = "Gemini CLI" }
)

foreach ($p in $npmPackages) {
    Write-Host "  Installing $($p.Name) ($($p.Pkg))..." -NoNewline
    if ($DryRun) { Write-Host " [DRY RUN]" -ForegroundColor DarkYellow; continue }
    npm install -g $p.Pkg --quiet 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-OK $p.Name }
    else { Write-Warn "$($p.Name) install may have failed — run: npm install -g $($p.Pkg)" }
}

# ─────────────────────────────────────────────
# 5. Container tooling
# ─────────────────────────────────────────────
Write-Step "Container tooling"
Install-WingetPackage -Id "Docker.DockerDesktop" -Name "Docker Desktop" -Skip:$SkipDocker

# ─────────────────────────────────────────────
# 6. VS Code extensions (AI + IaC)
# ─────────────────────────────────────────────
Write-Step "VS Code extensions"
if (Get-Command code -ErrorAction SilentlyContinue) {
    $extensions = @(
        "GitHub.copilot",
        "GitHub.copilot-chat",
        "ms-azuretools.vscode-bicep",
        "hashicorp.terraform",
        "ms-vscode.powershell",
        "ms-azuretools.azure-dev",
        "ms-azure-devops.azure-pipelines",
        "eamodio.gitlens",
        "GitHub.vscode-github-actions",
        "ms-vscode-remote.remote-containers",
        "ms-vscode-remote.remote-wsl",
        "redhat.ansible",
        "ms-python.python"
    )
    foreach ($ext in $extensions) {
        if (-not $DryRun) {
            code --install-extension $ext --force 2>&1 | Out-Null
        }
        Write-OK "Extension: $ext"
    }
} else {
    Write-Warn "VS Code CLI not found — skipping extension install. Run manually after adding 'code' to PATH."
}

# ─────────────────────────────────────────────
# 7. Summary
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Installation complete!" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next step: pwsh ./scripts/configure_repo.ps1" -ForegroundColor Yellow
Write-Host ""
