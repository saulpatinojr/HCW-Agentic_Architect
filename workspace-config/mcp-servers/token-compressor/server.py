from mcp.server.fastmcp import FastMCP
import re
import os

mcp = FastMCP("Token Squeezer & Caching Engine")

def minify_payload(content: str, file_type: str) -> str:
    """Strips tokens that LLMs do not need for comprehension."""
    if file_type in ['.json']:
        return re.sub(r'\s+', '', content)
    elif file_type in ['.tf', '.yaml', '.yml']:
        content = re.sub(r'#.*$', '', content, flags=re.MULTILINE)
        return re.sub(r'\n\s*\n', '\n', content)
    return content

@mcp.tool()
def read_compressed_file(filepath: str) -> str:
    """Reads a local file and minifies it for token efficiency."""
    if not os.path.exists(filepath):
        return "Error: File not found."
    
    _, ext = os.path.splitext(filepath)
    with open(filepath, 'r', encoding='utf-8') as f:
        raw_content = f.read()
    
    compressed = minify_payload(raw_content, ext)
    return f"""<context_block cache_control="ephemeral">
{compressed}
</context_block>"""

if __name__ == "__main__":
    mcp.run()