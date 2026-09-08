import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";

export function BulkActionsToolbar({
  selectedCount,
  onClear,
  children,
}: {
  selectedCount: number;
  onClear: () => void;
  children: ReactNode;
}) {
  if (selectedCount === 0) {
    return null;
  }

  return (
    <div className="bg-muted/50 mb-3 flex items-center gap-3 rounded-md border p-2">
      <span className="px-2 text-sm font-medium">{selectedCount} selected</span>
      <div className="flex items-center gap-2">{children}</div>
      <Button variant="ghost" size="sm" className="ml-auto" onClick={onClear}>
        Clear
      </Button>
    </div>
  );
}
