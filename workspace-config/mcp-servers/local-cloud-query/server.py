from mcp.server.fastmcp import FastMCP
import subprocess
import json

mcp = FastMCP("Local Cloud Environment Query")

@mcp.tool()
def run_terraform_plan(directory: str) -> str:
    """Runs terraform plan and returns the output for the TF-Engineer to review."""
    try:
        result = subprocess.run(
            ["terraform", "plan", "-no-color", "-out=tfplan"],
            cwd=directory,
            capture_output=True,
            text=True,
            check=True
        )
        return f"Plan Output:\n{result.stdout}"
    except subprocess.CalledProcessError as e:
        return f"Terraform Plan Failed:\n{e.stderr}"

@mcp.tool()
def get_aws_caller_identity() -> str:
    """Retrieves the current active AWS IAM identity for security verification."""
    try:
        result = subprocess.run(["aws", "sts", "get-caller-identity", "--output", "json"], capture_output=True, text=True)
        return result.stdout
    except Exception as e:
        return str(e)

if __name__ == "__main__":
    mcp.run_stdio()