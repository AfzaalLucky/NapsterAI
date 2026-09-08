import type {
  Agent,
  Companion,
  Connection,
  CreateAgentRequest,
  CreateConnectionRequest,
  CreateFaqCollectionRequest,
  CreateKnowledgeBaseRequest,
  FaqCollection,
  FaqItem,
  KnowledgeBase,
  PagedResult,
  ProblemDetails,
  Session,
} from "./types";

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!response.ok) {
    let detail = response.statusText;
    try {
      const problem = (await response.json()) as ProblemDetails;
      detail = problem.detail ?? problem.title ?? detail;
    } catch {
      // Non-JSON error body; fall back to statusText.
    }
    throw new ApiError(detail, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

function query(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") {
      search.set(key, String(value));
    }
  }
  const qs = search.toString();
  return qs ? `?${qs}` : "";
}

export const api = {
  browseCompanions(params: {
    search?: string;
    gender?: string;
    ethnicity?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<Companion>> {
    return request(`/companions${query(params)}`);
  },

  createAgent(body: CreateAgentRequest): Promise<Agent> {
    return request("/agents", { method: "POST", body: JSON.stringify(body) });
  },

  createConnection(body: CreateConnectionRequest): Promise<Connection> {
    return request("/connections", { method: "POST", body: JSON.stringify(body) });
  },

  listSessions(params: {
    companionId?: string;
    externalClientId?: string;
    sessionType?: string;
    search?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<Session>> {
    return request(`/sessions${query(params)}`);
  },

  listKnowledgeBases(params: {
    provider?: string;
    search?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<KnowledgeBase>> {
    return request(`/knowledgebases${query(params)}`);
  },

  createKnowledgeBase(body: CreateKnowledgeBaseRequest): Promise<KnowledgeBase> {
    return request("/knowledgebases", { method: "POST", body: JSON.stringify(body) });
  },

  listFaqCollections(params: {
    search?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<FaqCollection>> {
    return request(`/faqs${query(params)}`);
  },

  createFaqCollection(body: CreateFaqCollectionRequest): Promise<FaqCollection> {
    return request("/faqs", { method: "POST", body: JSON.stringify(body) });
  },

  listFaqItems(
    faqCollectionId: string,
    params: { pageIndex?: number; pageSize?: number },
  ): Promise<PagedResult<FaqItem>> {
    return request(`/faqs/${encodeURIComponent(faqCollectionId)}/items${query(params)}`);
  },
};
