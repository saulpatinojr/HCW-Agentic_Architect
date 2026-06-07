#!/usr/bin/env bash
# HCW Agentic Architect - macOS (Apple Silicon) CLI Installer
# Requires: Homebrew (https://brew.sh)
# Usage: bash scripts/install_cli.sh

set -euo pipefail

SKIP_AI=${SKIP_AI:-false}

log()  { echo -e "\033[0;36m  $1\033[0m"; }
ok()   { echo -e "\033[0;32m  OK $1\033[0m"; }
warn() { echo -e "\033[0;33m  WARN $1\033[0m"; }
head() { echo -e "\n\033[1;33m$1\033[0m"; }

echo ""
echo "=== HCW Agentic Architect - CLI Installer (macOS Apple Silicon) ==="
echo ""

if ! command -v brew &>/dev/null; then
    warn "Homebrew not found. Installing..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

head "[ 1/4 ] Core Cloud & IaC CLIs"
brew install azure-cli
brew install terraform
brew install gh
brew install --cask visual-studio-code
brew install --cask docker
brew install --cask github
az bicep install
ok "Core CLIs installed"

head "[ 2/4 ] Node.js Runtime"
brew install node
ok "Node.js installed: $(node --version)"

if [ "$SKIP_AI" != "true" ]; then
    head "[ 3/4 ] AI & Assistant CLIs"
    npm install -g @anthropic-ai/claude-code
    npm install -g @openai/codex
    npm install -g @google/gemini-cli
    ok "AI CLIs installed"
fi

head "[ 4/4 ] Ansible"
brew install ansible
ok "Ansible installed: $(ansible --version | head -1)"

echo ""
echo "=== Editor CLIs ==="
log "VS Code CLI (code): available after VS Code install"
log "Antigravity: install from https://antigravity.dev and add to PATH"

echo ""
echo "=== Verification ==="
for tool in az terraform gh code docker node claude gemini ansible; do
    if command -v "$tool" &>/dev/null; then
        echo "  OK $tool: $($tool --version 2>&1 | head -1)"
    else
        echo "  FAIL $tool: not found"
    fi
done

echo ""
echo "Installation complete. Next step: bash scripts/configure_repo.sh"
echo ""
