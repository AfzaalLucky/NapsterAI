import type * as React from "react";
import { Toaster as Sonner, type ToasterProps } from "sonner";

// Simplified from shadcn's generated version - that one reads next-themes, which this
// project doesn't depend on. "system" already follows prefers-color-scheme on its own.
function Toaster({ ...props }: ToasterProps) {
  return (
    <Sonner
      theme="system"
      className="toaster group"
      style={
        {
          "--normal-bg": "var(--popover)",
          "--normal-text": "var(--popover-foreground)",
          "--normal-border": "var(--border)",
        } as React.CSSProperties
      }
      {...props}
    />
  );
}

export { Toaster };
