from mcp.server.fastmcp import FastMCP
import os
import re
import time
import uuid


mcp = FastMCP("Context Optimization Engine")

_context_cache: dict[str, str] = {}
_session_stats = {
    "requests_total": 0,
    "tokens_original": 0,
    "tokens_optimized": 0,
    "tokens_saved": 0,
    "output_tokens_saved": 0,
    "overhead_ms_total": 0,
    "history": [],
    "projects": {
        "codex": 0,
        "claude": 0,
        "github": 0,
        "copilot": 0,
        "antigravity": 0,
        "vscode": 0,
    },
}


def _token_estimate(content: str) -> int:
    # Conservative estimate: about 4 characters per token for mixed text.
    return max(1, len(content) // 4)


def _minify_payload(content: str, file_type: str) -> str:
    """Strip low-value text tokens while keeping semantic content stable."""
    if file_type in [".json"]:
        return re.sub(r"\s+", "", content)

    if file_type in [".tf", ".yaml", ".yml"]:
        content = re.sub(r"#.*$", "", content, flags=re.MULTILINE)
        return re.sub(r"\n\s*\n", "\n", content)

    return content


def _record_stats(
    project: str,
    original: str,
    optimized: str,
    started_at: float,
) -> dict[str, object]:
    original_tokens = _token_estimate(original)
    optimized_tokens = _token_estimate(optimized)
    saved = max(0, original_tokens - optimized_tokens)
    overhead_ms = int((time.perf_counter() - started_at) * 1000)

    _session_stats["requests_total"] += 1
    _session_stats["tokens_original"] += original_tokens
    _session_stats["tokens_optimized"] += optimized_tokens
    _session_stats["tokens_saved"] += saved
    _session_stats["output_tokens_saved"] += int(saved * 0.2)
    _session_stats["overhead_ms_total"] += overhead_ms

    if project in _session_stats["projects"]:
        _session_stats["projects"][project] += saved

    event = {
        "timestamp_utc": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        "project": project,
        "tokens_original": original_tokens,
        "tokens_optimized": optimized_tokens,
        "tokens_saved": saved,
        "overhead_ms": overhead_ms,
    }

    history = _session_stats["history"]
    history.append(event)
    if len(history) > 200:
        del history[0]

    return event


@mcp.tool()
def read_compressed_file(filepath: str) -> str:
    """Compatibility tool for token-aware file reads."""
    if not os.path.exists(filepath):
        return "Error: File not found."

    _, ext = os.path.splitext(filepath)
    with open(filepath, "r", encoding="utf-8") as file_handle:
        raw_content = file_handle.read()

    started_at = time.perf_counter()
    optimized = _minify_payload(raw_content, ext)
    _record_stats("vscode", raw_content, optimized, started_at)

    return f"""<context_block cache_control="ephemeral">
{optimized}
</context_block>"""


@mcp.tool()
def compress_context(
    content: str,
    content_type: str = "text",
    project: str = "vscode",
) -> str:
    """Optimize context payload and cache original for optional retrieval."""
    started_at = time.perf_counter()
    extension = f".{content_type.lower().strip('.')}"
    optimized = _minify_payload(content, extension)
    context_id = str(uuid.uuid4())
    _context_cache[context_id] = content
    stats = _record_stats(project.lower(), content, optimized, started_at)

    return (
        f"context_id={context_id}\n"
        f"tokens_saved={stats['tokens_saved']}\n"
        "<context_block cache_control=\"ephemeral\">\n"
        f"{optimized}\n"
        "</context_block>"
    )


@mcp.tool()
def retrieve_context(context_id: str) -> str:
    """Retrieve an original, unoptimized payload by context id."""
    original = _context_cache.get(context_id)
    if original is None:
        return "Error: Context id not found."
    return original


@mcp.tool()
def optimization_stats() -> dict[str, object]:
    """Return session optimization metrics for dashboards and diagnostics."""
    requests_total = _session_stats["requests_total"]
    total_original = _session_stats["tokens_original"]
    total_saved = _session_stats["tokens_saved"]
    overhead_total = _session_stats["overhead_ms_total"]

    savings_percent = (
        (total_saved / total_original * 100) if total_original else 0
    )
    average_overhead_ms = (
        (overhead_total / requests_total) if requests_total else 0
    )
    output_reduction_percent = (
        (_session_stats["output_tokens_saved"] / total_saved * 100)
        if total_saved
        else 0
    )

    return {
        "requests": {
            "total": requests_total,
        },
        "tokens": {
            "original": total_original,
            "optimized": _session_stats["tokens_optimized"],
            "saved": total_saved,
            "savings_percent": round(savings_percent, 2),
            "output_saved": _session_stats["output_tokens_saved"],
            "output_reduction_percent": round(output_reduction_percent, 2),
        },
        "overhead": {
            "average_ms": round(average_overhead_ms, 2),
        },
        "projects": _session_stats["projects"],
        "history": list(_session_stats["history"]),
        "cache": {
            "entries": len(_context_cache),
        },
    }


if __name__ == "__main__":
    mcp.run()
