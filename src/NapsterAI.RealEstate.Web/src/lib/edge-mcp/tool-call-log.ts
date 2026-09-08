import { create } from "zustand";

export interface ToolCallLogEntry {
  id: number;
  name: string;
  args: unknown;
  result?: unknown;
  error?: string;
  latencyMs: number;
  timestamp: string;
}

const MAX_ENTRIES = 50;

interface ToolCallLogState {
  entries: ToolCallLogEntry[];
  add: (entry: Omit<ToolCallLogEntry, "id">) => void;
  clear: () => void;
}

let nextId = 0;

/** Backs the /webmcp-demo page's live tool-call log - see registerRealEstateTools.ts's withLogging(). */
export const useToolCallLog = create<ToolCallLogState>((set) => ({
  entries: [],
  add: (entry) =>
    set((state) => ({
      entries: [{ ...entry, id: nextId++ }, ...state.entries].slice(0, MAX_ENTRIES),
    })),
  clear: () => set({ entries: [] }),
}));
