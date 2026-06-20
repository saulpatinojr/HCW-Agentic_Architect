#!/usr/bin/env bash
# =============================================================================
# HCW Workspace Manager — macOS Apple Silicon CLI Installer
# Package manager: Homebrew
# Platform: macOS (arm64 / Apple silicon primary; Intel compatible)
#
# Usage:
#   chmod +x scripts/install_cli.sh
#   ./scripts/install_cli.sh
#   ./scripts/install_cli.sh --skip-docker --skip-antigravity
#
# After running: ./scripts/configure_repo.sh (or pwsh ./scripts/configure_repo.ps1)
# =============================================================================

set -euo pipefail

SKIP_DOCKER=false
SKIP_ANTIGRAVITY=false
SKIP_ANSIBLE=false
DRY_RUN=false

for arg in "$@"; do
  case $arg in
    --skip-docker)      SKIP_DOCKER=true ;;
    --skip-antigravity) SKIP_ANTIGRAVITY=true ;;
    --skip-ansible)     SKIP_ANSIBLE=true ;;
    --dry-run)          DRY_RUN=true ;;
  esac
done

step()  { echo; echo "==> $1"; }
ok()    { echo "  [OK] $1"; }
skip()  { echo "  [--] $1 (skipped)"; }
warn()  { echo "  [!!] $1"; }

brew_install() {
  local formula=$1
  local name=$2
  local skip=${3:-false}
  if [ "$skip" = true ]; then skip "$name"; return; fi
  echo -n "  Installing $name ($formula)..."
  if [ "$DRY_RUN" = true ]; then echo " [DRY RUN]"; return; fi
  if brew list "$formula" &>/dev/null; then
    echo " already installed"
    ok "$name"
  else
    brew install "$formula" --quiet && ok "$name" || warn "$name may have failed — check manually"
  fi
}

brew_cask_install() {
  local cask=$1
  local name=$2
  local skip=${3:-false}
  if [ "$skip" = true ]; then skip "$name"; return; fi
  echo -n "  Installing $name ($cask)..."
  if [ "$DRY_RUN" = true ]; then echo " [DRY RUN]"; return; fi
  if brew list --cask "$cask" &>/dev/null; then
    echo " already installed"
    ok "$name"
  else
    brew install --cask "$cask" --quiet && ok "$name" || warn "$name may have failed — check manually"
  fi
}

# ─────────────────────────────────────────────
# 0. Ensure Homebrew is installed
# ─────────────────────────────────────────────
step "Checking Homebrew"
if ! command -v brew &>/dev/null; then
  echo "  Homebrew not found. Installing..."
  if [ "$DRY_RUN" = false ]; then
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
    # Apple silicon path
    eval "$(/opt/homebrew/bin/brew shellenv)" 2>/dev/null || true
  fi
else
  ok "Homebrew $(brew --version | head -1)"
fi

# ─────────────────────────────────────────────
# 1. Core developer runtime
# ─────────────────────────────────────────────
step "Core developer runtime"
brew_install git          "Git"
brew_install gh           "GitHub CLI (gh)"
brew_install node         "Node.js LTS"
brew_install python@3.12  "Python 3.12"

# ─────────────────────────────────────────────
# 2. IaC + Azure toolchain
# ─────────────────────────────────────────────
step "IaC + Azure toolchain"
brew_install azure-cli    "Azure CLI (az)"
brew_install terraform    "Terraform"
brew_install packer       "Packer"
brew_install ansible      "Ansible" "$SKIP_ANSIBLE"

# Bicep via az
echo -n "  Installing Bicep via az bicep install..."
if [ "$DRY_RUN" = false ]; then
  az bicep install 2>/dev/null && ok "Bicep" || warn "az not on PATH yet — run 'az bicep install' after az setup"
else
  echo " [DRY RUN]"
fi

# ─────────────────────────────────────────────
# 3. IDE + editor CLIs
# ─────────────────────────────────────────────
step "IDE + editor CLIs"
brew_cask_install visual-studio-code  "VS Code"
brew_cask_install github              "GitHub Desktop"

# VS Code CLI
echo -n "  Checking VS Code CLI (code)..."
if command -v code &>/dev/null; then
  ok "VS Code CLI (code) on PATH"
else
  warn "'code' not on PATH. In VS Code: Cmd+Shift+P → 'Shell Command: Install code in PATH'"
fi

# Antigravity
if [ "$SKIP_ANTIGRAVITY" = true ]; then
  skip "Antigravity"
else
  echo -n "  Checking Antigravity CLI..."
  if command -v antigravity &>/dev/null; then
    ok "Antigravity CLI on PATH"
  else
    warn "Antigravity not in Homebrew. Install from https://antigravity.dev, then re-run to validate."
  fi
fi

# ─────────────────────────────────────────────
# 4. AI assistant CLIs (npm global)
# ─────────────────────────────────────────────
step "AI assistant CLIs (npm global)"
npm_packages=(
  "@anthropic-ai/claude-code:Claude Code CLI"
  "@openai/codex:OpenAI Codex CLI"
  "@google/gemini-cli:Gemini CLI"
)

for entry in "${npm_packages[@]}"; do
  pkg=${entry%%:*}
  name=${entry##*:}
  echo -n "  Installing $name ($pkg)..."
  if [ "$DRY_RUN" = true ]; then echo " [DRY RUN]"; continue; fi
  npm install -g "$pkg" --quiet 2>/dev/null && ok "$name" || warn "$name failed — run: npm install -g $pkg"
done

# ─────────────────────────────────────────────
# 5. Container tooling
# ─────────────────────────────────────────────
step "Container tooling"
brew_cask_install docker "Docker Desktop" "$SKIP_DOCKER"

# ─────────────────────────────────────────────
# 6. VS Code extensions
# ─────────────────────────────────────────────
step "VS Code extensions"
if command -v code &>/dev/null; then
  extensions=(
    "GitHub.copilot"
    "GitHub.copilot-chat"
    "ms-azuretools.vscode-bicep"
    "hashicorp.terraform"
    "ms-vscode.powershell"
    "ms-azuretools.azure-dev"
    "ms-azure-devops.azure-pipelines"
    "eamodio.gitlens"
    "GitHub.vscode-github-actions"
    "ms-vscode-remote.remote-containers"
    "redhat.ansible"
    "ms-python.python"
  )
  for ext in "${extensions[@]}"; do
    [ "$DRY_RUN" = false ] && code --install-extension "$ext" --force 2>/dev/null
    ok "Extension: $ext"
  done
else
  warn "VS Code CLI not found — skipping extensions. Add 'code' to PATH first."
fi

# ─────────────────────────────────────────────
# 7. Summary
# ─────────────────────────────────────────────
echo ""
echo "=========================================" 
echo " Installation complete!"
echo "========================================="
echo ""
echo "Next step: pwsh ./scripts/configure_repo.ps1"
echo "  (or source ./scripts/configure_repo.sh when available)"
echo ""
