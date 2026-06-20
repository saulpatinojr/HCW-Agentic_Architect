<#
.SYNOPSIS
    Interactive Agentic AI Workspace Orchestrator.
.DESCRIPTION
    Launches a custom interactive CLI portal to manage local workspace configurations,
    discover available agent packages, and align them with VS Code configurations.
#>

$RepoRoot = New-Object System.IO.DirectoryInfo( (Join-Path $PSScriptRoot "..") )
$AgentDir = Join-Path $RepoRoot.FullName "workspace-config\agents"
$VscodeDir = Join-Path $RepoRoot.FullName ".vscode"

if (-not (Test-Path $AgentDir)) { New-Item -Path $AgentDir -ItemType Directory | Out-Null }
if (-not (Test-Path $VscodeDir)) { New-Item -Path $VscodeDir -ItemType Directory | Out-Null }

function Show-CherubIntro {
    Clear-Host
    Write-Host @"
       ___      ________ 
     _/   \_   [========]  <-- The Hard Hat Group
    /       \  /  ____  \
   |  _   _  | | |    | |
   | (o) (o) |  \____/ /
   C    ^    D    /__/     "Greetings, Architect!"
    \  ===  /     
     \_____/      
     /     \     /| _ _ 
    / /| | \ \  / |( Y )   Your local cloud fabric is secure.
   ( < | | > )    | \ /    Now, it's time to build systems that think.
    \__|__|_/     V  V
"@ -ForegroundColor Cyan

    Write-Host "`n====================================================" -ForegroundColor DarkCyan
    Write-Host "   WELCOME BACK, AGENTIC AI ARCHITECT" -ForegroundColor White -BackgroundColor DarkBlue
    Write-Host "====================================================" -ForegroundColor DarkCyan
    Write-Host " Status: Ready to assemble the intelligent layers.`n" -ForegroundColor Gray
}

function Get-AvailableAgents {
    $Directories = Get-ChildItem -Path $AgentDir | Where-Object { $_.PSIsContainer }
    $AgentList = @()
    
    foreach ($Dir in $Directories) {
        $AgentsMd = Join-Path $Dir.FullName "AGENTS.md"
        $FriendlyName = $Dir.Name
        if (Test-Path $AgentsMd) {
            $FirstLine = Get-Content -Path $AgentsMd -TotalCount 2 | Where-Object { $_ -match "^# " }
            if ($FirstLine) { $FriendlyName = $FirstLine -replace "^#\s*Persona:\s*|^#\s*", "" }
        }
        
        $AgentList += [PSCustomObject]@{
            DirectoryName = $Dir.Name
            FullPath      = $Dir.FullName
            FriendlyName  = $FriendlyName
        }
    }
    return $AgentList
}

function Import-AgentPack {
    Write-Host "`n[*] AGENT PACK INGESTION PROTOCOL" -ForegroundColor Cyan
    Write-Host "====================================================" -ForegroundColor DarkCyan
    
    $ZipPath = Read-Host "Enter the full path to the Agent Pack .zip file"
    $ZipPath = $ZipPath.Trim('"').Trim("'")

    if (-not (Test-Path $ZipPath)) {
        Write-Host "  [-] Error: Could not locate file at $ZipPath" -ForegroundColor Red
        Pause; return
    }

    $PackName = [System.IO.Path]::GetFileNameWithoutExtension($ZipPath)
    $TargetAgentDir = Join-Path $AgentDir $PackName

    if (Test-Path $TargetAgentDir) {
        Write-Host "  [!] Agent Pack '$PackName' already exists." -ForegroundColor Yellow
        $Overwrite = Read-Host "  Overwrite existing pack? (y/N)"
        if ($Overwrite -ne 'y') { return }
        Remove-Item -Path $TargetAgentDir -Recurse -Force
    }

    Write-Host "  [*] Unpacking payload..." -ForegroundColor Gray
    Expand-Archive -Path $ZipPath -DestinationPath $TargetAgentDir -Force
    
    if (Test-Path (Join-Path $TargetAgentDir "AGENTS.md")) {
        Write-Host "  [+] Successfully ingested Agent Pack: $PackName" -ForegroundColor Green
    } else {
        Write-Host "  [!] Warning: Pack extracted, but no primary 'AGENTS.md' was found inside." -ForegroundColor Magenta
    }
    Pause
}

function Update-AgentToolchain {
    param([string]$AgentPackFolder)
    
    $SourceDir = Join-Path $AgentDir $AgentPackFolder
    Write-Host "  [*] Binding toolchain configurations for: $AgentPackFolder" -ForegroundColor Gray

    # 1. Bind GitHub Copilot Instructions
    $SourceCopilot = Join-Path $SourceDir ".github\copilot-instructions.md"
    $TargetCopilotDir = Join-Path $RepoRoot.FullName ".github"
    if (Test-Path $SourceCopilot) {
        if (-not (Test-Path $TargetCopilotDir)) { New-Item -Path $TargetCopilotDir -ItemType Directory | Out-Null }
        Copy-Item $SourceCopilot -Destination (Join-Path $TargetCopilotDir "copilot-instructions.md") -Force
        Write-Host "      -> GitHub Copilot Context [ACTIVE]" -ForegroundColor Green
    }

    # 2. Bind Claude Code Instructions
    $SourceClaude = Join-Path $SourceDir "CLAUDE.md"
    if (Test-Path $SourceClaude) {
        Copy-Item $SourceClaude -Destination (Join-Path $RepoRoot.FullName "CLAUDE.md") -Force
        Write-Host "      -> Claude CLI Context     [ACTIVE]" -ForegroundColor Green
    }

    # 3. Bind Codex Instructions
    $SourceCodex = Join-Path $SourceDir ".codex\instructions.md"
    $TargetCodexDir = Join-Path $RepoRoot.FullName ".codex"
    if (Test-Path $SourceCodex) {
        if (-not (Test-Path $TargetCodexDir)) { New-Item -Path $TargetCodexDir -ItemType Directory | Out-Null }
        Copy-Item $SourceCodex -Destination (Join-Path $TargetCodexDir "instructions.md") -Force
        Write-Host "      -> Codex CLI Context      [ACTIVE]" -ForegroundColor Green
    }
}

function Invoke-TeamAssembling {
    Show-CherubIntro
    $Agents = Get-AvailableAgents

    if ($Agents.Count -eq 0) {
        Write-Host "[!] Your 'agents/' group is empty." -ForegroundColor Yellow
        Pause; return
    }

    Write-Host "[*] DISCOVERED AGENT PACKS (Select to Active Core Workspace):" -ForegroundColor Yellow
    for ($i = 0; $i -lt $Agents.Count; $i++) {
        Write-Host "  $($i + 1)) [$($Agents[$i].DirectoryName)] -> $($Agents[$i].FriendlyName)" -ForegroundColor White
    }
    Write-Host "  B) Go Back" -ForegroundColor Gray
    Write-Host ""
    
    $Selection = Read-Host "Assemble your workspace selection"
    if ($Selection -ieq 'B') { return }
    
    if ($Selection -match '^\d+$' -and [int]$Selection -le $Agents.Count) {
        $Picked = $Agents[[int]$Selection - 1]
        
        # Build VS Code Workspace Schema tracking
        $McpPath = Join-Path $VscodeDir "mcp.json"
        $McpConfig = @{
            mcpServers = @{}
            activePersonas = @($Picked.FriendlyName)
        }
        $McpConfig | ConvertTo-Json -Depth 10 | Out-File $McpPath -Encoding utf8
        
        # Link Toolchains locally
        Update-AgentToolchain $Picked.DirectoryName
        
        # Map to Runtime appdata
        $AppDataRoaming = [Environment]::GetFolderPath("ApplicationData")
        $TargetMcpLink = Join-Path $AppDataRoaming "Code\User\mcp.json"
        if (Test-Path $TargetMcpLink) { Remove-Item $TargetMcpLink -Force }
        Copy-Item $McpPath -Destination $TargetMcpLink -Force
        
        Write-Host "`n[✓] Workspace selection synced live to local VS Code environment." -ForegroundColor Green
    } else {
        Write-Host "Invalid Selection." -ForegroundColor Red
    }
    Pause
}

function Invoke-SystemCheck {
    Write-Host "`n[*] INITIATING HOST CAPABILITY SCAN..." -ForegroundColor Cyan
    Write-Host "====================================================" -ForegroundColor DarkCyan

    $Toolchain = [ordered]@{
        "Cloud Providers"   = @(
            @{ Name="AWS CLI"; Cmd="aws" },
            @{ Name="Azure CLI"; Cmd="az" },
            @{ Name="Google Cloud CLI"; Cmd="gcloud" }
        )
        "Service Providers" = @(
            @{ Name="Terraform"; Cmd="terraform" },
            @{ Name="Ansible"; Cmd="ansible" },
            @{ Name="Kubernetes CLI"; Cmd="kubectl" },
            @{ Name="Helm"; Cmd="helm" },
            @{ Name="Docker"; Cmd="docker" },
            @{ Name="GitHub CLI"; Cmd="gh" }
        )
        "AI Providers"      = @(
            @{ Name="Claude Code CLI"; Cmd="claude" },
            @{ Name="GitHub Copilot CLI"; Cmd="gh copilot" },
            @{ Name="Codex CLI"; Cmd="codex" }
        )
    }

    $MissingDependencies = 0
    foreach ($Category in $Toolchain.Keys) {
        Write-Host "`n  [$Category]" -ForegroundColor Yellow
        foreach ($Tool in $Toolchain[$Category]) {
            if ($Tool.Cmd -eq "gh copilot") {
                $null = & gh copilot --help 2>$null
                $CommandExists = $LASTEXITCODE -eq 0
            } else {
                $CommandExists = Get-Command $Tool.Cmd -ErrorAction SilentlyContinue
            }

            if ($CommandExists) {
                Write-Host "   [✓] $($Tool.Name.PadRight(20)) -> $($Tool.Cmd)" -ForegroundColor Green
            } else {
                Write-Host "   [X] $($Tool.Name.PadRight(20)) -> $($Tool.Cmd) (MISSING)" -ForegroundColor Red
                $MissingDependencies++
            }
        }
    }

    Write-Host "`n====================================================" -ForegroundColor DarkCyan
    if ($MissingDependencies -eq 0) {
        Write-Host "[SYSTEM READY] All infrastructure tool paths verified." -ForegroundColor Green
    } else {
        Write-Host "[WARNING] $MissingDependencies tools are missing from environment paths." -ForegroundColor Magenta
    }
    Pause
}

do {
    Show-CherubIntro
    Write-Host "1) Active and Mount Agent Pack to VS Code"
    Write-Host "2) Ingest New Agent Pack (.zip)"
    Write-Host "3) System Dependency Check"
    Write-Host "Q) Stand Down / Close Portal"
    Write-Host ""
    $MenuInput = Read-Host "Action Required"

    switch ($MenuInput) {
        "1" { Invoke-TeamAssembling }
        "2" { Import-AgentPack }
        "3" { Invoke-SystemCheck }
        "q" { $Exit = $true }
        "Q" { $Exit = $true }
    }
} while (-not $Exit)