# Skill: Scaffold a New Model Context Protocol (MCP) Server
**Trigger:** User requests to build, scaffold, or integrate a new MCP server.

### Execution Steps
1. **Verify Local Environment:** Run a silent check for Node.js (`node -v`) or Python (`python --version`) depending on the requested MCP language stack.
2. **Initialize Scaffolding:** Navigate to `/mcp-servers/`. Create a sub-folder named after the target service (e.g., `/mcp-servers/aws-ec2-manager`). Generate the foundational code layout.
3. **Define MCP Primitives:** Implement the required MCP server initialization sequence. Define at least one `tool` capability using strict JSON schema validation for inputs.
4. **Integrate Locally:** Output the exact JSON snippet the user needs to append to their `.vscode/mcp.json` file to register the new server.