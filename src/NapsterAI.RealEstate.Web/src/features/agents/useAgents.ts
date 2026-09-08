import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { CreateRealEstateAgentRequest, UpdateRealEstateAgentRequest } from "@/api/types";

export function useAgentsQuery(organizationId?: number) {
  return useQuery({
    queryKey: ["agents", organizationId],
    queryFn: () => api.agents.list({ organizationId }),
  });
}

export function useCreateAgent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateRealEstateAgentRequest) => api.agents.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["agents"] }),
  });
}

export function useUpdateAgent(salesAgentId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateRealEstateAgentRequest) => api.agents.update(salesAgentId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["agents"] }),
  });
}

export function useDeleteAgent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (salesAgentId: number) => api.agents.delete(salesAgentId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["agents"] }),
  });
}
