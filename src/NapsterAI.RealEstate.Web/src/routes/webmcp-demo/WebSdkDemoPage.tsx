import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getModelContext } from "@/lib/edge-mcp/model-context";
import { useResourceStore } from "@/lib/edge-mcp/resource-store";
import { useToolCallLog } from "@/lib/edge-mcp/tool-call-log";
import { formatPrice } from "@/lib/format";

function useRegisteredToolNames(): string[] {
  const [names, setNames] = useState<string[]>([]);

  useEffect(() => {
    const mc = getModelContext();
    if (!mc) return;

    let cancelled = false;
    const refresh = () => {
      mc.getTools().then((tools) => {
        if (!cancelled) setNames(tools.map((t) => t.name));
      });
    };

    refresh();
    mc.addEventListener("toolchange", refresh);
    return () => {
      cancelled = true;
      mc.removeEventListener("toolchange", refresh);
    };
  }, []);

  return names;
}

export function WebSdkDemoPage() {
  const toolNames = useRegisteredToolNames();
  const entries = useToolCallLog((state) => state.entries);
  const clearLog = useToolCallLog((state) => state.clear);
  const searchResults = useResourceStore((state) => state.resources["re://search-results"]);

  const modelContextAvailable = getModelContext() !== null;

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
      <h1 className="text-2xl font-semibold">WebMCP Demo</h1>
      <p className="text-muted-foreground mt-2">
        Open the chat widget (bottom-right) and ask something like{" "}
        <em>"Show me 2 bedroom units in Dubai under 2,000,000 AED"</em> or{" "}
        <em>"What amenities does Marina Vista Towers have?"</em> - each tool call the AI makes appears live below.
      </p>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>document.modelContext</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant={modelContextAvailable ? "default" : "destructive"}>
              {modelContextAvailable ? "Available" : "Not available"}
            </Badge>
            {toolNames.length > 0 ? (
              toolNames.map((name) => (
                <Badge key={name} variant="outline">
                  {name}
                </Badge>
              ))
            ) : (
              <span className="text-muted-foreground text-sm">No tools registered yet.</span>
            )}
          </div>
        </CardContent>
      </Card>

      {searchResults && (
        <Card className="mt-6">
          <CardHeader>
            <CardTitle>Live resource: re://search-results</CardTitle>
          </CardHeader>
          <CardContent>
            <SearchResultsGrid value={searchResults.value} />
          </CardContent>
        </Card>
      )}

      <Card className="mt-6">
        <CardHeader className="flex-row items-center justify-between space-y-0">
          <CardTitle>Tool call log</CardTitle>
          <Button variant="outline" size="sm" onClick={clearLog} disabled={entries.length === 0}>
            Clear
          </Button>
        </CardHeader>
        <CardContent>
          {entries.length === 0 ? (
            <p className="text-muted-foreground text-sm">No tool calls yet.</p>
          ) : (
            <div className="flex flex-col gap-3">
              {entries.map((entry) => (
                <div key={entry.id} className="rounded-md border p-3 text-sm">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="font-mono font-semibold">{entry.name}</span>
                    <Badge variant={entry.error ? "destructive" : "secondary"}>
                      {entry.error ? "error" : "ok"}
                    </Badge>
                    <span className="text-muted-foreground text-xs">{entry.latencyMs}ms</span>
                    <span className="text-muted-foreground text-xs">
                      {new Date(entry.timestamp).toLocaleTimeString()}
                    </span>
                  </div>
                  <pre className="bg-muted mt-2 overflow-x-auto rounded p-2 text-xs">
                    args: {JSON.stringify(entry.args, null, 2)}
                  </pre>
                  <pre className="bg-muted mt-2 overflow-x-auto rounded p-2 text-xs">
                    {entry.error ? `error: ${entry.error}` : `result: ${JSON.stringify(entry.result, null, 2)}`}
                  </pre>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

interface InventoryLikeItem {
  inventoryId: number;
  unitNumber: string;
  bedrooms?: number;
  listPrice?: number;
  status: string;
}

function SearchResultsGrid({ value }: { value: unknown }) {
  const items = (value as { items?: InventoryLikeItem[] } | null)?.items ?? [];

  if (items.length === 0) {
    return <p className="text-muted-foreground text-sm">No matching units.</p>;
  }

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
      {items.map((item) => (
        <div key={item.inventoryId} className="flex items-center justify-between rounded-md border p-3 text-sm">
          <div>
            <p className="font-medium">Unit {item.unitNumber}</p>
            <p className="text-muted-foreground text-xs">
              {item.bedrooms ?? "—"} bed · {item.status}
            </p>
          </div>
          {item.listPrice != null && <p className="font-semibold">{formatPrice(item.listPrice, "AED")}</p>}
        </div>
      ))}
    </div>
  );
}
