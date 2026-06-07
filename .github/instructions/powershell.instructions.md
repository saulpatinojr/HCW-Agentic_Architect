---
applyTo: "**/*.ps1"
---
# PowerShell Agent Instructions

Follow AGENTS.md. Additional PowerShell-specific rules:

1. `#Requires -Version 7.0` at top of every script
2. Use `[CmdletBinding()]` and `param()` blocks — no positional-only params
3. Error handling: `$ErrorActionPreference = 'Stop'` + try/catch blocks
4. No Write-Host in production scripts — use Write-Output / Write-Verbose
5. Secrets: retrieve from Key Vault only — `Get-AzKeyVaultSecret`
6. Use approved PowerShell verbs (Get-, Set-, New-, Remove-, Invoke-)
7. Include comment-based help (.SYNOPSIS, .DESCRIPTION, .PARAMETER, .EXAMPLE)
