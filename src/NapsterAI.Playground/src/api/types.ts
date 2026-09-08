// Mirrors src/NapsterAI.Api/Models/Dtos/*.cs — keep in sync with the backend DTOs.

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  filteredCount: number;
  pageIndex: number;
  pageSize: number;
}

export interface Companion {
  id: string;
  firstName: string;
  lastName: string;
  previewUrl?: string;
  videoLoopUrl?: string;
  ethnicity?: string;
  gender?: string;
  headline?: string;
  tags: Record<string, string>;
  status?: string;
}

export interface CreateAgentRequest {
  companionId: string;
  name: string;
  providerSettings: unknown;
  language?: string;
  voiceId?: string;
  functions?: string[];
  mcp?: unknown;
  faqCollections?: string[];
  knowledgeBaseId?: string;
  tags?: unknown;
  disableIdleTimeout?: boolean;
  useWebSearch?: boolean;
}

export interface Agent {
  id: string;
  companionId: string;
  name: string;
  previewUrl?: string;
  language?: string;
  voiceId?: string;
  functions: string[];
  disableIdleTimeout?: boolean;
  useWebSearch?: boolean;
  created?: number;
}

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

export interface CreateKnowledgeBaseRequest {
  name: string;
  provider?: string;
}

export interface KnowledgeBase {
  id: string;
  name: string;
  provider?: string;
  itemsCount?: number;
  tags: Record<string, string>;
  created?: number;
}

export interface FaqItemRequest {
  question: string;
  answer: string;
}

export interface CreateFaqCollectionRequest {
  name: string;
  faqs?: FaqItemRequest[];
}

export interface FaqCollection {
  id: string;
  name: string;
  itemsCount?: number;
  created?: number;
}

export interface FaqItem {
  id: string;
  question: string;
  answer: string;
  createdAt?: number;
}

export interface Session {
  id: string;
  companionId?: string;
  companionFirstName?: string;
  companionLastName?: string;
  externalClientId?: string;
  sessionType?: string;
  modality?: string;
  status?: string;
  agentName?: string;
  closeReason?: string;
  cost?: number;
  createdAt?: number;
  startedAt?: number;
  closedAt?: number;
}

/** Shape of an RFC 7807 problem+json error body returned by the API's exception middleware. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}
