<#
.SYNOPSIS
    HCW Agentic Architect - Repo Configuration Script
.DESCRIPTION
    Configures local environment: Azure CLI auth, GitHub CLI auth,
    creates AGENTS.local.md with prompted values (gitignored),
    and installs VS Code recommended extensions.
.NOTES
    Run after install_cli.ps1. Requires PowerShell 7+.
#>
#Requires -Version 7.0

[CmdletBinding()]
param(
    [switch]$SkipAzureAuth,
    [switch]$SkipGitHubAuth,
    [switch]$SkipExtensions
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path $PSScriptRoot -Parent

Write-Host "`n=== HCW Agentic Architect - Repo Configuration ===" -ForegroundColor Green

# GitHub CLI Auth
if (-not $SkipGitHubAuth) {
    Write-Host "`n[ 1/4 ] GitHub CLI Authentication" -ForegroundColor Yellow
    $ghStatus = gh auth status 2>&1
    if ($ghStatus -match "Logged in") {
        Write-Host "  Already authenticated with GitHub CLI" -ForegroundColor Green
    } else {
        gh auth login --web --git-protocol https
    }
}

# Azure CLI Auth
if (-not $SkipAzureAuth) {
    Write-Host "`n[ 2/4 ] Azure CLI Authentication" -ForegroundColor Yellow
    $azAccount = az account show 2>&1
    if ($azAccount -match "environmentName") {
        Write-Host "  Already authenticated with Azure CLI" -ForegroundColor Green
        az account show --query "{Name:name, Subscription:id, Tenant:tenantId}" -o table
    } else {
        az login
    }
}

# AGENTS.local.md
Write-Host "`n[ 3/4 ] Local Config (AGENTS.local.md)" -ForegroundColor Yellow
$localConfig = Join-Path $RepoRoot "AGENTS.local.md"

if (Test-Path $localConfig) {
    Write-Host "  AGENTS.local.md already exists — skipping" -ForegroundColor Green
} else {
    $tenantId = az account show --query tenantId -o tsv 2>$null
    if (-not $tenantId) { $tenantId = Read-Host "  Enter Azure Tenant ID" }
    $subId = az account show --query id -o tsv 2>$null
    if (-not $subId) { $subId = Read-Host "  Enter Azure Subscription ID" }
    $subName = az account show --query name -o tsv 2>$null
    if (-not $subName) { $subName = Read-Host "  Enter Azure Subscription Name" }
    $region = Read-Host "  Default Azure region [eastus2]"
    if (-not $region) { $region = "eastus2" }

    $content = @"
# AGENTS.local.md
# LOCAL MACHINE ONLY — DO NOT COMMIT — gitignored
# Created: $(Get-Date -Format "yyyy-MM-dd")

AZURE_TENANT_ID=$tenantId
AZURE_SUBSCRIPTION_ID=$subId
AZURE_SUBSCRIPTION_NAME=$subName
AZURE_DEFAULT_REGION=$region
GITHUB_ORG=saulpatinojr
GITHUB_REPO=HCW-Agentic_Architect
"@
    Set-Content -Path $localConfig -Value $content -Encoding UTF8
    Write-Host "  AGENTS.local.md created" -ForegroundColor Green
}

# VS Code Extensions
if (-not $SkipExtensions) {
    Write-Host "`n[ 4/4 ] VS Code Extensions" -ForegroundColor Yellow
    $extensions = @(
        "github.copilot"
        "github.copilot-chat"
        "ms-azuretools.vscode-bicep"
        "hashicorp.terraform"
        "ms-vscode.powershell"
        "ms-vscode.azure-account"
        "ms-azuretools.vscode-azureresourcegroups"
        "eamodio.gitlens"
        "github.vscode-github-actions"
        "ms-vscode-remote.remote-wsl"
        "ms-vscode-remote.remote-containers"
        "redhat.vscode-yaml"
        "ms-vscode.vscode-node-azure-pack"
    )
    foreach ($ext in $extensions) {
        try {
            code --install-extension $ext --force 2>&1 | Out-Null
            Write-Host "  OK $ext" -ForegroundColor Green
        } catch {
            Write-Host "  WARN $ext — install manually" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n=== Configuration Complete ===" -ForegroundColor Green
Write-Host "  Repo:    https://github.com/saulpatinojr/HCW-Agentic_Architect"
Write-Host "  VS Code: code $RepoRoot"
Write-Host "  Antigravity: antigravity open $RepoRoot"
Write-Host ""
