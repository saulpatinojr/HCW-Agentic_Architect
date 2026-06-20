---
applyTo: "**/*.ps1,**/*.psm1,**/*.psd1"
---

# PowerShell File Instructions

These instructions apply to all PowerShell files in this repository.

## Runtime Target

- PowerShell 7.4+ (pwsh) only — NOT Windows PowerShell 5.x
- Always add at top of script:
```powershell
#Requires -Version 7.4
```

## Required Structure

Every script must have:
```powershell
[CmdletBinding(SupportsShouldProcess)]
param (
    [Parameter(Mandatory)]
    [string]$RequiredParam
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
```

## Error Handling

Always use try/catch/finally:
```powershell
try {
    # operation
} catch {
    Write-Error "[FAIL] $($_.Exception.Message)"
    throw
}
```

## Secrets

- Use `$env:VARIABLE_NAME` for all secrets
- Never hardcode passwords, keys, or subscription IDs
- Use `[securestring]` type for sensitive parameters
