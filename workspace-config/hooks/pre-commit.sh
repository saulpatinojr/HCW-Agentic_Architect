#!/bin/bash
# Workspace Manager Zero-Trust Pre-Commit Hook

echo "[*] Initiating workspace pre-commit scan..."

STAGED_TF=$(git diff --cached --name-only --diff-filter=ACM | grep "\.tf$")

if [ -z "$STAGED_TF" ]; then
    echo "[✓] No Terraform files modified. Proceeding."
    exit 0
fi

echo "[*] Staged Terraform files detected. Running formatting check..."
terraform fmt -check
if [ $? -ne 0 ]; then
    echo "[-] Error: Terraform formatting failed. Run 'terraform fmt'."
    exit 1
fi

echo "[*] Invoking Claude Code CLI for Zero-Trust and FinOps review..."
CLAUDE_REVIEW=$(claude "Read the current staged git diff. Act as an Information Security Administrator. If there are any plaintext secrets, wide-open security groups (0.0.0.0/0), or missing FinOps allocation tags, reply strictly with 'REJECT:' followed by the reason. If it looks secure, reply strictly with 'APPROVE'.")

if [[ "$CLAUDE_REVIEW" == *"REJECT:"* ]]; then
    echo -e "\n🚨 SECURITY/FINOPS VIOLATION DETECTED BY AGENT 🚨"
    echo "$CLAUDE_REVIEW"
    echo "Commit blocked."
    exit 1
fi

echo "[✓] Workspace review passed. Committing code."
exit 0
