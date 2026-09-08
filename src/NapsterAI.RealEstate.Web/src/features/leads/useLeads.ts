import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { AssignLeadRequest, CreateLeadActivityRequest, UpdateLeadStatusRequest } from "@/api/types";

export interface LeadSearchParams {
  status?: string;
  salesAgentId?: number;
  projectId?: number;
  pageIndex?: number;
  pageSize?: number;
}

export function useLeadsQuery(params: LeadSearchParams) {
  return useQuery({
    queryKey: ["leads", params],
    queryFn: () => api.leads.list(params),
  });
}

export function useLeadQuery(leadId: number) {
  return useQuery({
    queryKey: ["leads", leadId],
    queryFn: () => api.leads.get(leadId),
    enabled: Number.isFinite(leadId),
  });
}

export function useLeadActivitiesQuery(leadId: number) {
  return useQuery({
    queryKey: ["leads", leadId, "activities"],
    queryFn: () => api.leads.listActivities(leadId),
    enabled: Number.isFinite(leadId),
  });
}

export function useUpdateLeadStatus(leadId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateLeadStatusRequest) => api.leads.updateStatus(leadId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
}

export function useAssignLead(leadId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: AssignLeadRequest) => api.leads.assign(leadId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
}

export function useAddLeadActivity(leadId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateLeadActivityRequest) => api.leads.addActivity(leadId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads", leadId, "activities"] });
      queryClient.invalidateQueries({ queryKey: ["leads", leadId] });
    },
  });
}
