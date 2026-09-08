import { useAuthStore } from "@/lib/auth-store";
import type {
  Amenity,
  AssignLeadRequest,
  AuthResult,
  CalculatePaymentPlanRequest,
  Connection,
  CreateAmenityRequest,
  CreateConnectionRequest,
  CreateInquiryRequest,
  CreateInventoryRequest,
  CreateLeadActivityRequest,
  CreateLeadRequest,
  CreateOrganizationRequest,
  CreatePaymentPlanMilestoneRequest,
  CreateProjectRequest,
  CreateRealEstateAgentRequest,
  CreateUnitTypeRequest,
  CreateViewingRequest,
  Inquiry,
  Inventory,
  InventorySummary,
  Lead,
  LeadActivity,
  LeadsSummary,
  Location,
  Lookup,
  LoginRequest,
  Media,
  Organization,
  PagedResult,
  PaymentPlanMilestone,
  PaymentPlanSchedule,
  Project,
  ProblemDetails,
  RealEstateAgent,
  RefreshTokenRequest,
  UnitType,
  UpdateAmenityRequest,
  UpdateInventoryRequest,
  UpdateLeadStatusRequest,
  UpdateOrganizationRequest,
  UpdateProjectRequest,
  UpdateRealEstateAgentRequest,
  UpdateUnitTypeRequest,
  UpdateViewingStatusRequest,
  Viewing,
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
  const accessToken = useAuthStore.getState().accessToken;

  const response = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      // The access token is missing/expired/invalid - drop the stale session so RequireAuth
      // sends the user back to /login rather than looping on 401s.
      useAuthStore.getState().logout();
    }

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

function query(params: Record<string, string | number | boolean | undefined>): string {
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
  auth: {
    login(body: LoginRequest): Promise<AuthResult> {
      return request("/auth/login", { method: "POST", body: JSON.stringify(body) });
    },
    refresh(body: RefreshTokenRequest): Promise<AuthResult> {
      return request("/auth/refresh", { method: "POST", body: JSON.stringify(body) });
    },
  },

  // Platform-wide, not RealEstate-prefixed - same unmodified endpoint NapsterAI.Playground's
  // own client calls. Backs RealEstateChatWidget.tsx.
  connections: {
    create(body: CreateConnectionRequest): Promise<Connection> {
      return request("/connections", { method: "POST", body: JSON.stringify(body) });
    },
  },

  projects: {
    list(params: {
      city?: string;
      projectType?: string;
      status?: string;
      isFeatured?: boolean;
      minPrice?: number;
      maxPrice?: number;
      search?: string;
      pageIndex?: number;
      pageSize?: number;
    }): Promise<PagedResult<Project>> {
      return request(`/realestate/projects${query(params)}`);
    },
    get(id: number): Promise<Project> {
      return request(`/realestate/projects/${id}`);
    },
    unitTypes(id: number): Promise<UnitType[]> {
      return request(`/realestate/projects/${id}/unit-types`);
    },
    amenities(id: number): Promise<Amenity[]> {
      return request(`/realestate/projects/${id}/amenities`);
    },
    media(id: number): Promise<Media[]> {
      return request(`/realestate/projects/${id}/media`);
    },
    paymentPlanMilestones(id: number): Promise<PaymentPlanMilestone[]> {
      return request(`/realestate/projects/${id}/payment-plan-milestones`);
    },
    addPaymentPlanMilestone(id: number, body: CreatePaymentPlanMilestoneRequest): Promise<PaymentPlanMilestone> {
      return request(`/realestate/projects/${id}/payment-plan-milestones`, {
        method: "POST",
        body: JSON.stringify(body),
      });
    },
    create(body: CreateProjectRequest): Promise<Project> {
      return request("/realestate/projects", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateProjectRequest): Promise<Project> {
      return request(`/realestate/projects/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    approve(id: number): Promise<Project> {
      return request(`/realestate/projects/${id}/approve`, { method: "POST" });
    },
    reject(id: number): Promise<Project> {
      return request(`/realestate/projects/${id}/reject`, { method: "POST" });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/projects/${id}`, { method: "DELETE" });
    },
  },

  unitTypes: {
    list(params: { projectId?: number }): Promise<UnitType[]> {
      return request(`/realestate/unit-types${query(params)}`);
    },
    get(id: number): Promise<UnitType> {
      return request(`/realestate/unit-types/${id}`);
    },
    create(body: CreateUnitTypeRequest): Promise<UnitType> {
      return request("/realestate/unit-types", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateUnitTypeRequest): Promise<UnitType> {
      return request(`/realestate/unit-types/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/unit-types/${id}`, { method: "DELETE" });
    },
  },

  inventory: {
    list(params: {
      projectId?: number;
      unitTypeId?: number;
      minBedrooms?: number;
      maxBedrooms?: number;
      minPrice?: number;
      maxPrice?: number;
      minAreaSqFt?: number;
      maxAreaSqFt?: number;
      status?: string;
      viewType?: string;
      furnishingStatus?: string;
      search?: string;
      sortBy?: string;
      sortDescending?: boolean;
      pageIndex?: number;
      pageSize?: number;
    }): Promise<PagedResult<Inventory>> {
      return request(`/realestate/inventory${query(params)}`);
    },
    get(id: number): Promise<Inventory> {
      return request(`/realestate/inventory/${id}`);
    },
    calculatePaymentPlan(id: number, body: CalculatePaymentPlanRequest): Promise<PaymentPlanSchedule> {
      return request(`/realestate/inventory/${id}/payment-plan`, { method: "POST", body: JSON.stringify(body) });
    },
    create(body: CreateInventoryRequest): Promise<Inventory> {
      return request("/realestate/inventory", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateInventoryRequest): Promise<Inventory> {
      return request(`/realestate/inventory/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/inventory/${id}`, { method: "DELETE" });
    },
  },

  amenities: {
    list(params: { projectId?: number; category?: string }): Promise<Amenity[]> {
      return request(`/realestate/amenities${query(params)}`);
    },
    get(id: number): Promise<Amenity> {
      return request(`/realestate/amenities/${id}`);
    },
    create(body: CreateAmenityRequest): Promise<Amenity> {
      return request("/realestate/amenities", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateAmenityRequest): Promise<Amenity> {
      return request(`/realestate/amenities/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/amenities/${id}`, { method: "DELETE" });
    },
  },

  locations: {
    list(params: { country?: string; city?: string }): Promise<Location[]> {
      return request(`/realestate/locations${query(params)}`);
    },
  },

  lookups: {
    list(params: { type?: string }): Promise<Lookup[]> {
      return request(`/realestate/lookups${query(params)}`);
    },
  },

  organizations: {
    list(): Promise<Organization[]> {
      return request("/realestate/organizations");
    },
    get(id: number): Promise<Organization> {
      return request(`/realestate/organizations/${id}`);
    },
    create(body: CreateOrganizationRequest): Promise<Organization> {
      return request("/realestate/organizations", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateOrganizationRequest): Promise<Organization> {
      return request(`/realestate/organizations/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/organizations/${id}`, { method: "DELETE" });
    },
  },

  agents: {
    list(params: { organizationId?: number }): Promise<RealEstateAgent[]> {
      return request(`/realestate/agents${query(params)}`);
    },
    get(id: number): Promise<RealEstateAgent> {
      return request(`/realestate/agents/${id}`);
    },
    create(body: CreateRealEstateAgentRequest): Promise<RealEstateAgent> {
      return request("/realestate/agents", { method: "POST", body: JSON.stringify(body) });
    },
    update(id: number, body: UpdateRealEstateAgentRequest): Promise<RealEstateAgent> {
      return request(`/realestate/agents/${id}`, { method: "PUT", body: JSON.stringify(body) });
    },
    delete(id: number): Promise<void> {
      return request(`/realestate/agents/${id}`, { method: "DELETE" });
    },
  },

  leads: {
    list(params: {
      status?: string;
      salesAgentId?: number;
      projectId?: number;
      pageIndex?: number;
      pageSize?: number;
    }): Promise<PagedResult<Lead>> {
      return request(`/realestate/leads${query(params)}`);
    },
    get(id: number): Promise<Lead> {
      return request(`/realestate/leads/${id}`);
    },
    create(body: CreateLeadRequest): Promise<Lead> {
      return request("/realestate/leads", { method: "POST", body: JSON.stringify(body) });
    },
    updateStatus(id: number, body: UpdateLeadStatusRequest): Promise<Lead> {
      return request(`/realestate/leads/${id}/status`, { method: "PATCH", body: JSON.stringify(body) });
    },
    assign(id: number, body: AssignLeadRequest): Promise<Lead> {
      return request(`/realestate/leads/${id}/assign`, { method: "POST", body: JSON.stringify(body) });
    },
    listActivities(id: number): Promise<LeadActivity[]> {
      return request(`/realestate/leads/${id}/activities`);
    },
    addActivity(id: number, body: CreateLeadActivityRequest): Promise<LeadActivity> {
      return request(`/realestate/leads/${id}/activities`, { method: "POST", body: JSON.stringify(body) });
    },
  },

  inquiries: {
    list(params: { projectId?: number; pageIndex?: number; pageSize?: number }): Promise<PagedResult<Inquiry>> {
      return request(`/realestate/inquiries${query(params)}`);
    },
    get(id: number): Promise<Inquiry> {
      return request(`/realestate/inquiries/${id}`);
    },
    create(body: CreateInquiryRequest): Promise<Inquiry> {
      return request("/realestate/inquiries", { method: "POST", body: JSON.stringify(body) });
    },
    convertToLead(id: number): Promise<Lead> {
      return request(`/realestate/inquiries/${id}/convert-to-lead`, { method: "POST" });
    },
  },

  viewings: {
    list(params: {
      leadId?: number;
      inventoryId?: number;
      status?: string;
      pageIndex?: number;
      pageSize?: number;
    }): Promise<PagedResult<Viewing>> {
      return request(`/realestate/viewings${query(params)}`);
    },
    get(id: number): Promise<Viewing> {
      return request(`/realestate/viewings/${id}`);
    },
    create(body: CreateViewingRequest): Promise<Viewing> {
      return request("/realestate/viewings", { method: "POST", body: JSON.stringify(body) });
    },
    updateStatus(id: number, body: UpdateViewingStatusRequest): Promise<Viewing> {
      return request(`/realestate/viewings/${id}/status`, { method: "PATCH", body: JSON.stringify(body) });
    },
  },

  analytics: {
    leadsSummary(): Promise<LeadsSummary> {
      return request("/realestate/analytics/leads-summary");
    },
    inventorySummary(): Promise<InventorySummary> {
      return request("/realestate/analytics/inventory-summary");
    },
  },
};
