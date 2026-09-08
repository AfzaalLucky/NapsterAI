// Mirrors src/NapsterAI.Api/Models/Dtos/**/*.cs — keep in sync with the backend DTOs.

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  filteredCount: number;
  pageIndex: number;
  pageSize: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

// --- Connections (ConnectionDtos.cs) - platform-wide, not RealEstate-prefixed ---------
// Brokers a live WebRTC/WebSocket session token for a Napster companion. Backs
// RealEstateChatWidget.tsx; shared unmodified with NapsterAI.Playground's own client.

export interface CreateConnectionRequest {
  companionId: string;
  providerConfig: unknown;
  videoPolicy?: string;
  disableIdleTimeout?: boolean;
  useWebSearch?: boolean;
  functions?: string[];
  knowledgeBaseId?: string;
  externalClientId?: string;
  language?: string;
  initialSpeech?: string;
}

export interface Connection {
  token: string;
  connectionId: string;
}

// --- Auth (AuthDtos.cs) ------------------------------------------------

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AuthResult {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  email: string;
  role: string;
}

// --- Projects (ProjectDtos.cs) ------------------------------------------

export interface Project {
  projectId: number;
  projectCode: string;
  projectName: string;
  developer?: string;
  projectType: string;
  status: string;
  description?: string;
  country: string;
  city: string;
  district?: string;
  address?: string;
  latitude?: number;
  longitude?: number;
  totalBuildings?: number;
  totalFloors?: number;
  totalUnits?: number;
  launchDate?: string;
  constructionStart?: string;
  estimatedCompletion?: string;
  handoverDate?: string;
  startingPrice?: number;
  maxPrice?: number;
  currency: string;
  paymentPlan?: string;
  permitNumber?: string;
  serviceCharge?: number;
  masterPlanUrl?: string;
  brochureUrl?: string;
  imageUrl?: string;
  videoUrl?: string;
  contactPerson?: string;
  contactPhone?: string;
  contactEmail?: string;
  isFeatured: boolean;
  isActive: boolean;
  approvalStatus: string;
  createdDate: string;
  modifiedDate?: string;
}

export type CreateProjectRequest = Omit<
  Project,
  "projectId" | "isActive" | "approvalStatus" | "createdDate" | "modifiedDate"
>;

export type UpdateProjectRequest = CreateProjectRequest & { isActive: boolean };

// --- Unit Types (UnitTypeDtos.cs) ---------------------------------------

export interface UnitType {
  unitTypeId: number;
  projectId: number;
  typeName: string;
  category: string;
  bedrooms?: number;
  bathrooms?: number;
  minAreaSqFt?: number;
  maxAreaSqFt?: number;
  basePrice?: number;
  pricePerSqFt?: number;
  totalUnitsOfType?: number;
  availableUnitsOfType?: number;
  floorPlanUrl?: string;
  description?: string;
}

export type CreateUnitTypeRequest = Omit<UnitType, "unitTypeId">;
export type UpdateUnitTypeRequest = CreateUnitTypeRequest;

// --- Inventory (InventoryDtos.cs) ---------------------------------------

export interface Inventory {
  inventoryId: number;
  projectId: number;
  unitTypeId: number;
  unitNumber: string;
  buildingTower?: string;
  floorNumber?: number;
  viewType?: string;
  areaSqFt?: number;
  bedrooms?: number;
  bathrooms?: number;
  parkingSpaces?: number;
  hasBalcony?: boolean;
  furnishingStatus?: string;
  listPrice?: number;
  pricePerSqFt?: number;
  status: string;
  listingDate?: string;
  soldOrLeasedDate?: string;
  buyerTenantName?: string;
  salesAgentId?: number;
  agentName?: string;
  agentContact?: string;
  notes?: string;
  approvalStatus: string;
}

export type CreateInventoryRequest = Omit<
  Inventory,
  "inventoryId" | "soldOrLeasedDate" | "buyerTenantName" | "approvalStatus"
>;
export type UpdateInventoryRequest = CreateInventoryRequest & {
  soldOrLeasedDate?: string;
  buyerTenantName?: string;
};

// --- Amenities (AmenityDtos.cs) -----------------------------------------

export interface Amenity {
  amenityId: number;
  projectId: number;
  amenityName: string;
  category: string;
  description?: string;
  iconUrl?: string;
  imageUrl?: string;
  isHighlighted: boolean;
  displayOrder: number;
}

export type CreateAmenityRequest = Omit<Amenity, "amenityId">;
export type UpdateAmenityRequest = CreateAmenityRequest;

// --- Media (MediaDtos.cs) ------------------------------------------------

export interface Media {
  mediaId: number;
  entityType: string;
  entityId: number;
  mediaType: string;
  url: string;
  displayOrder: number;
  isPrimary: boolean;
}

// --- Locations / Lookups (LocationDtos.cs, LookupDtos.cs) ---------------

export interface Location {
  locationId: number;
  country: string;
  city: string;
  district?: string;
  latitude?: number;
  longitude?: number;
}

export interface Lookup {
  lookupId: number;
  lookupType: string;
  code: string;
  displayName: string;
  displayOrder: number;
}

// --- Organizations (OrganizationDtos.cs) ---------------------------------

export interface Organization {
  organizationId: number;
  name: string;
  licenseNumber?: string;
  logoUrl?: string;
  phone?: string;
  email?: string;
  website?: string;
  isActive: boolean;
}

export type CreateOrganizationRequest = Omit<Organization, "organizationId" | "isActive">;
export type UpdateOrganizationRequest = CreateOrganizationRequest & { isActive: boolean };

// --- Sales Agents (RealEstateAgentDtos.cs) -------------------------------

export interface RealEstateAgent {
  salesAgentId: number;
  organizationId: number;
  organizationName?: string;
  fullName: string;
  phone?: string;
  email?: string;
  photoUrl?: string;
  licenseNumber?: string;
  isActive: boolean;
}

export type CreateRealEstateAgentRequest = Omit<
  RealEstateAgent,
  "salesAgentId" | "organizationName" | "isActive"
>;
export type UpdateRealEstateAgentRequest = CreateRealEstateAgentRequest & { isActive: boolean };

// --- Customers (CustomerDto.cs) - read-only, embedded in Lead/Inquiry ----

export interface Customer {
  customerId: number;
  fullName: string;
  email: string;
  phone?: string;
  nationality?: string;
  preferredLanguage?: string;
  source?: string;
}

// --- Leads (LeadDtos.cs) --------------------------------------------------

export interface Lead {
  leadId: number;
  customer: Customer;
  projectId?: number;
  inventoryId?: number;
  salesAgentId?: number;
  salesAgentName?: string;
  status: string;
  source?: string;
  budget?: number;
  requirementsNotes?: string;
  createdDate: string;
  lastContactedDate?: string;
}

export interface CreateLeadRequest {
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
  projectId?: number;
  inventoryId?: number;
  source?: string;
  budget?: number;
  requirementsNotes?: string;
}

export interface UpdateLeadStatusRequest {
  status: string;
}

export interface AssignLeadRequest {
  salesAgentId: number;
}

export interface CreateLeadActivityRequest {
  activityType: string;
  notes?: string;
}

export interface LeadActivity {
  leadActivityId: number;
  activityType: string;
  notes?: string;
  createdByUserEmail?: string;
  createdDate: string;
}

// --- Inquiries (InquiryDtos.cs) -------------------------------------------

export interface Inquiry {
  inquiryId: number;
  customer: Customer;
  projectId?: number;
  inventoryId?: number;
  channel: string;
  message?: string;
  createdDate: string;
  convertedToLeadId?: number;
}

export interface CreateInquiryRequest {
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
  projectId?: number;
  inventoryId?: number;
  channel: string;
  message?: string;
}

// --- Viewings (ViewingDtos.cs) --------------------------------------------

export interface Viewing {
  viewingId: number;
  leadId: number;
  inventoryId: number;
  scheduledDate: string;
  status: string;
  salesAgentId?: number;
  notes?: string;
}

export interface CreateViewingRequest {
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
  inventoryId: number;
  scheduledDate: string;
  notes?: string;
}

export interface UpdateViewingStatusRequest {
  status: string;
}

// --- Payment plans (PaymentPlanDtos.cs) -----------------------------------

export interface PaymentPlanMilestone {
  milestoneId: number;
  projectId: number;
  milestoneName: string;
  percentDue: number;
  triggerEvent?: string;
  dueDateOffsetDays?: number;
  displayOrder: number;
}

export type CreatePaymentPlanMilestoneRequest = Omit<PaymentPlanMilestone, "milestoneId" | "projectId">;

export interface CalculatePaymentPlanRequest {
  downPaymentPercent?: number;
}

export interface PaymentPlanScheduleItem {
  milestoneName: string;
  percentDue: number;
  amountDue: number;
  triggerEvent?: string;
  dueDateOffsetDays?: number;
}

export interface PaymentPlanSchedule {
  inventoryId: number;
  projectId: number;
  listPrice: number;
  currency: string;
  milestones: PaymentPlanScheduleItem[];
}

// --- Analytics (AnalyticsDtos.cs) ------------------------------------------

export interface LeadsSummary {
  totalLeads: number;
  byStatus: Record<string, number>;
  viewingsThisWeek: number;
  conversionRatePercent?: number;
  pipelineValue: number;
}

export interface InventorySummary {
  totalProjects: number;
  totalUnits: number;
  byStatus: Record<string, number>;
  totalListValue: number;
}

// --- Enum-like constants (mirrors the C# *Statuses/*Types static classes) -

export const LeadStatuses = [
  "New",
  "Contacted",
  "Qualified",
  "ViewingScheduled",
  "Negotiation",
  "Won",
  "Lost",
] as const;

export const ViewingStatuses = ["Requested", "Confirmed", "Completed", "Cancelled", "NoShow"] as const;

export const LeadActivityTypes = ["Note", "Call", "Email", "ViewingBooked", "StatusChange"] as const;

export const LeadSources = ["Website", "AI Assistant", "Call", "WalkIn"] as const;

export const ApprovalStatuses = ["Draft", "PendingReview", "Approved", "Rejected"] as const;

export const InventoryStatuses = ["Available", "Reserved", "Sold", "Blocked", "Leased"] as const;

export const ProjectTypes = ["Residential", "Commercial", "Mixed-Use", "Villa", "Retail"] as const;

export const ProjectStatuses = ["Planning", "Under Construction", "Ready", "Handover", "Sold Out"] as const;
