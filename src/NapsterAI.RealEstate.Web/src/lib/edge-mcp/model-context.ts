import type { ModelContextWithExtensions } from "@mcp-b/webmcp-types";

/**
 * `document.modelContext` typed against MCP-B's extended `registerTool()` overload (the base
 * WebMCP `ModelContextTool.annotations` only carries readOnlyHint/untrustedContentHint - the
 * destructiveHint/idempotentHint the EdgeMcpBridge also reads come from this extension).
 * Returns null before `@mcp-b/webmcp-polyfill` has installed the property.
 */
export function getModelContext(): ModelContextWithExtensions | null {
  return (document.modelContext as ModelContextWithExtensions | undefined) ?? null;
}
