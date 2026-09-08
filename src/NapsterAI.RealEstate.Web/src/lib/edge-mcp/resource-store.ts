import { create } from "zustand";

export interface ResourceEntry {
  uri: string;
  name: string;
  value: unknown;
}

interface ResourceStoreState {
  resources: Record<string, ResourceEntry>;
  set: (uri: string, name: string, value: unknown) => void;
}

/** React-facing mirror of the resources published below - backs the /webmcp-demo page's live grid. */
export const useResourceStore = create<ResourceStoreState>((set) => ({
  resources: {},
  set: (uri, name, value) =>
    set((state) => ({
      resources: { ...state.resources, [uri]: { uri, name, value } },
    })),
}));

type ResourceUpdateHandler = (update: { uri: string; value: unknown }) => void;

const subscribers = new Map<string, Set<ResourceUpdateHandler>>();

/** Updates a resource's value and notifies both the React store and any document.modelContext subscribers (the EdgeMcpBridge, once attached). */
export function publishResource(uri: string, name: string, value: unknown) {
  useResourceStore.getState().set(uri, name, value);
  for (const handler of subscribers.get(uri) ?? []) {
    handler({ uri, value });
  }
}

interface ResourceExtension {
  getResources?: () => Array<{ uri: string; name: string }>;
  readResource?: (uri: string) => Promise<unknown>;
  subscribeResource?: (uri: string, handler: ResourceUpdateHandler) => () => void;
}

/**
 * Installs the Napster toolkit's optional live-state resource extension onto
 * `document.modelContext` (getResources/readResource/subscribeResource). This is NOT part of
 * the base WebMCP spec - `@mcp-b/webmcp-polyfill` deliberately only implements tools; resources
 * belong to the heavier `@mcp-b/global` MCP-B runtime, which Phase 9 doesn't otherwise need.
 * The EdgeMcpBridge (inside @touchcastllc/napster-companion-api) feature-detects these three
 * methods and simply skips live-state relay when they're absent, so implementing just this one
 * `re://search-results` resource directly here is a reasonable, low-dependency middle ground.
 */
export function installResourceExtension(): void {
  const mc = document.modelContext as (typeof document.modelContext & ResourceExtension) | undefined;
  if (!mc || mc.getResources) {
    return; // no modelContext yet, or already installed
  }

  mc.getResources = () =>
    Object.values(useResourceStore.getState().resources).map(({ uri, name }) => ({ uri, name }));

  mc.readResource = async (uri: string) => useResourceStore.getState().resources[uri]?.value ?? null;

  mc.subscribeResource = (uri: string, handler: ResourceUpdateHandler) => {
    let set = subscribers.get(uri);
    if (!set) {
      set = new Set();
      subscribers.set(uri, set);
    }
    set.add(handler);
    return () => set.delete(handler);
  };
}
