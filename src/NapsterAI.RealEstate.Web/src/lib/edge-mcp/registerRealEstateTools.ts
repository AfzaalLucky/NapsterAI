import { initializeWebMCPPolyfill } from "@mcp-b/webmcp-polyfill";
import { ApiError, api } from "@/api/client";
import type { CreateViewingRequest } from "@/api/types";
import { getModelContext } from "./model-context";
import { installResourceExtension, publishResource } from "./resource-store";
import { useToolCallLog } from "./tool-call-log";

let registered = false;

/** Wraps a tool's execute() to time it and record it to the /webmcp-demo live log, success or failure. */
function withLogging<TArgs, TResult>(name: string, fn: (args: TArgs) => Promise<TResult>) {
  return async (args: TArgs): Promise<TResult> => {
    const start = performance.now();
    try {
      const result = await fn(args);
      useToolCallLog.getState().add({
        name,
        args,
        result,
        latencyMs: Math.round(performance.now() - start),
        timestamp: new Date().toISOString(),
      });
      return result;
    } catch (error) {
      useToolCallLog.getState().add({
        name,
        args,
        error: error instanceof ApiError ? error.message : error instanceof Error ? error.message : "Unknown error",
        latencyMs: Math.round(performance.now() - start),
        timestamp: new Date().toISOString(),
      });
      throw error;
    }
  };
}

type InventorySearchParams = Parameters<typeof api.inventory.list>[0];

/** GET /inventory only filters by a single projectId - fan out and merge when a city resolves to several projects. */
async function searchAcrossProjects(projectIds: number[], params: Omit<InventorySearchParams, "projectId">) {
  const pages = await Promise.all(projectIds.map((projectId) => api.inventory.list({ ...params, projectId })));
  const items = pages.flatMap((page) => page.items);
  return { items, totalCount: items.length, filteredCount: items.length, pageIndex: 0, pageSize: items.length };
}

/**
 * getProjectDetails/getAmenities only take a numeric projectId, but a visitor (or the AI acting
 * on their behalf) naturally names a project ("Marina Vista Towers"), not its id. Resolve a name
 * to an id via the projects search endpoint rather than leaving the caller with no valid way to
 * look it up - without this, the AI has been observed guessing/inventing an answer instead of
 * calling a tool at all when asked about a project purely by name.
 */
async function resolveProjectId(args: { projectId?: number; projectName?: string }): Promise<number> {
  if (args.projectId) return args.projectId;
  if (!args.projectName) {
    throw new Error("Either projectId or projectName is required.");
  }

  const matches = await api.projects.list({ search: args.projectName, pageSize: 5 });
  if (matches.items.length === 0) {
    throw new Error(`No project found matching "${args.projectName}".`);
  }
  return matches.items[0].projectId;
}

interface SearchInventoryArgs {
  city?: string;
  projectId?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minPrice?: number;
  maxPrice?: number;
  status?: string;
  viewType?: string;
  sortBy?: string;
}

/**
 * Registers the Real Estate module's 7 EdgeMCP tools on `document.modelContext` (installing
 * `@mcp-b/webmcp-polyfill` first if the browser doesn't natively support WebMCP), and the
 * optional live-state resource extension the EdgeMcpBridge feature-detects. The bridge inside
 * @touchcastllc/napster-companion-api (already invoked by NapsterCompanionApiSdk.init() in
 * RealEstateChatWidget.tsx) auto-discovers these - no bridge code to write here. All tools run
 * with anonymous-visitor privileges only (the same endpoints the public site itself calls) -
 * no admin tools are exposed.
 */
export function registerRealEstateTools(): void {
  if (registered) {
    return;
  }
  registered = true;

  initializeWebMCPPolyfill();
  installResourceExtension();

  const mc = getModelContext();
  if (!mc) {
    console.warn("[registerRealEstateTools] document.modelContext is unavailable - EdgeMCP tools were not registered.");
    return;
  }

  const signal = new AbortController().signal;

  mc.registerTool(
    {
      name: "searchInventory",
      description: "Search available real estate units by city, project, bedrooms, price range, view, or status.",
      inputSchema: {
        type: "object",
        properties: {
          city: { type: "string", description: "City or district to search in, e.g. 'Dubai Marina'." },
          projectId: { type: "number", description: "Limit results to a specific project id." },
          minBedrooms: { type: "number" },
          maxBedrooms: { type: "number" },
          minPrice: { type: "number" },
          maxPrice: { type: "number" },
          status: { type: "string", description: "Available, Reserved, Sold, Blocked, or Leased." },
          viewType: { type: "string", description: "e.g. Sea View, Marina View, City View." },
          sortBy: { type: "string", description: "price, area, or date." },
        },
      },
      annotations: { readOnlyHint: true },
      execute: withLogging("searchInventory", async (args: SearchInventoryArgs) => {
        const params: Omit<InventorySearchParams, "projectId"> = {
          minBedrooms: args.minBedrooms,
          maxBedrooms: args.maxBedrooms,
          minPrice: args.minPrice,
          maxPrice: args.maxPrice,
          status: args.status,
          viewType: args.viewType,
          sortBy: args.sortBy,
          pageSize: 10,
        };

        let result: Awaited<ReturnType<typeof api.inventory.list>>;

        if (args.projectId) {
          result = await api.inventory.list({ ...params, projectId: args.projectId });
        } else if (args.city) {
          const projects = await api.projects.list({ city: args.city, pageSize: 20 });
          const projectIds = projects.items.map((p) => p.projectId);
          result =
            projectIds.length > 0
              ? await searchAcrossProjects(projectIds, params)
              : { items: [], totalCount: 0, filteredCount: 0, pageIndex: 0, pageSize: 0 };
        } else {
          result = await api.inventory.list(params);
        }

        publishResource("re://search-results", "Search Results", result);
        return result;
      }),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "getProjectDetails",
      description:
        "Get full details for a specific real estate project, including its amenities. Pass either projectId (if already known, e.g. from a prior searchInventory result) or projectName (the development's name, e.g. 'Marina Vista Towers', as given by the visitor).",
      inputSchema: {
        type: "object",
        properties: {
          projectId: { type: "number" },
          projectName: { type: "string", description: "The project's name, if projectId isn't already known." },
        },
      },
      annotations: { readOnlyHint: true },
      execute: withLogging("getProjectDetails", async (args: { projectId?: number; projectName?: string }) => {
        const projectId = await resolveProjectId(args);
        const [project, amenities] = await Promise.all([
          api.projects.get(projectId),
          api.projects.amenities(projectId).catch(() => []),
        ]);
        return { ...project, amenities };
      }),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "calculatePaymentPlan",
      description: "Calculate a payment milestone schedule for a specific unit, optionally with a down payment percentage.",
      inputSchema: {
        type: "object",
        properties: {
          inventoryId: { type: "number" },
          downPaymentPercent: { type: "number", description: "0-100, defaults to the plan's own schedule if omitted." },
        },
        required: ["inventoryId"],
      },
      annotations: { readOnlyHint: true, idempotentHint: true },
      execute: withLogging(
        "calculatePaymentPlan",
        async (args: { inventoryId: number; downPaymentPercent?: number }) =>
          api.inventory.calculatePaymentPlan(args.inventoryId, { downPaymentPercent: args.downPaymentPercent }),
      ),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "bookViewing",
      description:
        "Book a property viewing for a customer. Confirm the unit, date, and time with the customer before calling this.",
      inputSchema: {
        type: "object",
        properties: {
          inventoryId: { type: "number" },
          customerName: { type: "string" },
          customerEmail: { type: "string" },
          customerPhone: { type: "string" },
          scheduledDate: { type: "string", description: "ISO 8601 date-time, e.g. 2026-09-20T14:00:00Z." },
          notes: { type: "string" },
        },
        required: ["inventoryId", "customerName", "customerEmail", "scheduledDate"],
      },
      annotations: { destructiveHint: true },
      execute: withLogging("bookViewing", async (args: CreateViewingRequest) => api.viewings.create(args)),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "createLead",
      description: "Capture a customer's contact details and interest as a lead for a sales agent to follow up on.",
      inputSchema: {
        type: "object",
        properties: {
          customerName: { type: "string" },
          customerEmail: { type: "string" },
          customerPhone: { type: "string" },
          projectId: { type: "number" },
          inventoryId: { type: "number" },
          message: { type: "string" },
        },
        required: ["customerName", "customerEmail"],
      },
      annotations: { destructiveHint: true },
      execute: withLogging(
        "createLead",
        async (args: { customerName: string; customerEmail: string; customerPhone?: string; projectId?: number; inventoryId?: number; message?: string }) =>
          api.inquiries.create({ ...args, channel: "AI Assistant" }),
      ),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "getAmenities",
      description:
        "List the amenities and facilities offered by a specific project. Pass either projectId or projectName (the development's name, e.g. 'Marina Vista Towers', as given by the visitor).",
      inputSchema: {
        type: "object",
        properties: {
          projectId: { type: "number" },
          projectName: { type: "string", description: "The project's name, if projectId isn't already known." },
        },
      },
      annotations: { readOnlyHint: true },
      execute: withLogging("getAmenities", async (args: { projectId?: number; projectName?: string }) =>
        api.projects.amenities(await resolveProjectId(args)),
      ),
    },
    { signal },
  );

  mc.registerTool(
    {
      name: "checkAvailability",
      description: "Check the current availability and status of a specific unit by its id.",
      inputSchema: {
        type: "object",
        properties: { inventoryId: { type: "number" } },
        required: ["inventoryId"],
      },
      annotations: { readOnlyHint: true, idempotentHint: true },
      execute: withLogging("checkAvailability", async (args: { inventoryId: number }) => api.inventory.get(args.inventoryId)),
    },
    { signal },
  );
}
