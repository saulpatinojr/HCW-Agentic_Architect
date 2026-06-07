<#
.SYNOPSIS
    HCW Agentic Architect - Windows CLI Installer
.DESCRIPTION
    Installs all required CLIs for the HCW Agentic Architect platform on Windows.
    Uses winget as primary package manager. Requires PowerShell 7+.
.NOTES
    Run as Administrator for best results.
    After running, execute: .\scripts\configure_repo.ps1
#>
#Requires -Version 7.0

[CmdletBinding()]
param(
    [switch]$SkipAI,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

function Install-WithWinget {
    [CmdletBinding()]
    param([string]$Id, [string]$Name)
    Write-Host "  Installing $Name..." -ForegroundColor Cyan
    if ($WhatIf) { Write-Host "  [WhatIf] winget install --id $Id -e --accept-source-agreements --accept-package-agreements" }
    else { winget install --id $Id -e --accept-source-agreements --accept-package-agreements --silent }
}

function Install-WithNpm {
    [CmdletBinding()]
    param([string]$Package, [string]$Name)
    Write-Host "  Installing $Name (npm)..." -ForegroundColor Cyan
    if ($WhatIf) { Write-Host "  [WhatIf] npm install -g $Package" }
    else { npm install -g $Package }
}

Write-Host "`n=== HCW Agentic Architect - CLI Installer (Windows) ===" -ForegroundColor Green
Write-Host "Platform: Windows x64/ARM | Package Manager: winget`n"

# Core Cloud & IaC
Write-Host "[ 1/5 ] Core Cloud & IaC CLIs" -ForegroundColor Yellow
Install-WithWinget "Microsoft.AzureCLI"         "Azure CLI"
Install-WithWinget "Hashicorp.Terraform"         "Terraform"
Install-WithWinget "GitHub.cli"                  "GitHub CLI (gh)"
Install-WithWinget "Microsoft.VisualStudioCode"  "VS Code"
Install-WithWinget "Docker.DockerDesktop"        "Docker Desktop"
Install-WithWinget "GitHub.GitHubDesktop"        "GitHub Desktop"
Install-WithWinget "OpenTofu.OpenTofu"           "OpenTofu (optional)"
if (-not $WhatIf) { az bicep install }

# Node.js
Write-Host "`n[ 2/5 ] Node.js Runtime" -ForegroundColor Yellow
Install-WithWinget "OpenJS.NodeJS.LTS" "Node.js LTS"

# AI CLIs
if (-not $SkipAI) {
    Write-Host "`n[ 3/5 ] AI & Assistant CLIs" -ForegroundColor Yellow
    Install-WithNpm "@anthropic-ai/claude-code"  "Claude Code CLI"
    Install-WithNpm "@openai/codex"              "OpenAI Codex CLI"
    Install-WithNpm "@google/gemini-cli"         "Gemini CLI"
}

# Editor CLIs
Write-Host "`n[ 4/5 ] Editor CLIs" -ForegroundColor Yellow
Write-Host "  VS Code CLI (code) installed with VS Code — verify: code --version"
Write-Host "  Antigravity: install from https://antigravity.dev and ensure 'antigravity' is in PATH"

# Ansible
Write-Host "`n[ 5/5 ] Ansible" -ForegroundColor Yellow
Write-Host "  Ansible runs best in WSL2 on Windows." -ForegroundColor DarkYellow
Write-Host "  To install: wsl -e bash -c 'sudo apt update && sudo apt install -y ansible'"

# Verify
Write-Host "`n=== Verification ===" -ForegroundColor Green
$checks = @(
    @{ Cmd = "az"; Args = "--version"; Name = "Azure CLI" }
    @{ Cmd = "terraform"; Args = "--version"; Name = "Terraform" }
    @{ Cmd = "gh"; Args = "--version"; Name = "GitHub CLI" }
    @{ Cmd = "code"; Args = "--version"; Name = "VS Code CLI" }
    @{ Cmd = "docker"; Args = "--version"; Name = "Docker" }
    @{ Cmd = "node"; Args = "--version"; Name = "Node.js" }
    @{ Cmd = "claude"; Args = "--version"; Name = "Claude Code CLI" }
    @{ Cmd = "gemini"; Args = "--version"; Name = "Gemini CLI" }
)
foreach ($c in $checks) {
    try {
        $v = & $c.Cmd $c.Args 2>&1 | Select-Object -First 1
        Write-Host "  OK $($c.Name): $v" -ForegroundColor Green
    } catch {
        Write-Host "  FAIL $($c.Name): not found" -ForegroundColor Red
    }
}

Write-Host "`nInstallation complete. Next step: .\scripts\configure_repo.ps1`n" -ForegroundColor Green
