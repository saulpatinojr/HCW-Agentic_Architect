# PowerShell Agent Instructions

> Scope: All `.ps1`, `.psm1`, `.psd1` files.
> Inherits from: `../../AGENTS.md`

## Runtime

- **Target**: PowerShell 7.4+ (pwsh) — NOT Windows PowerShell 5.x
- **Cross-platform**: Scripts must run on Windows (primary), macOS, and Linux
- **Execution Policy**: Use `pwsh -ExecutionPolicy Bypass -File script.ps1` in CI

## Style Standards

```powershell
#Requires -Version 7.4
#Requires -Modules Az.Accounts, Az.Resources

<#
.SYNOPSIS
    Short one-line description.
.DESCRIPTION
    Full description of what the script does.
.PARAMETER ResourceGroup
    The target Azure resource group name.
.EXAMPLE
    .\Deploy-Resources.ps1 -ResourceGroup rg-lz-dev-eus2
#>
[CmdletBinding(SupportsShouldProcess)]
param (
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$ResourceGroup,

    [Parameter()]
    [ValidateSet('dev','tst','stg','prd')]
    [string]$Environment = 'dev'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
```

## Error Handling Pattern

```powershell
try {
    $result = Invoke-AzCommand
    Write-Host "[OK] Operation succeeded" -ForegroundColor Green
}
catch {
    Write-Error "[FAIL] $($_.Exception.Message)"
    throw
}
finally {
    # Cleanup always runs
}
```

## Azure Auth Pattern

```powershell
# Check if already authenticated
$context = Get-AzContext
if (-not $context) {
    Write-Host "Authenticating to Azure..."
    Connect-AzAccount -UseDeviceAuthentication
}

# Always set subscription explicitly
Set-AzContext -SubscriptionId $env:AZURE_SUBSCRIPTION_ID
```

## Rules

- Always use `[CmdletBinding()]` and `param()` blocks
- Always set `$ErrorActionPreference = 'Stop'`
- Use `Write-Verbose` for debug output, `Write-Host` for user-facing status
- Use `SupportsShouldProcess` and `-WhatIf` for destructive operations
- Never use `Write-Host` for data output — use `Write-Output` or return objects
- Prefer pipeline-compatible functions (accept input from pipeline)
- Use `$env:` variables for secrets, never hardcoded strings
