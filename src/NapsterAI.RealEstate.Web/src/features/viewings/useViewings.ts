import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { UpdateViewingStatusRequest } from "@/api/types";

export interface ViewingSearchParams {
  leadId?: number;
  inventoryId?: number;
  status?: string;
  pageIndex?: number;
  pageSize?: number;
}

export function useViewingsQuery(params: ViewingSearchParams) {
  return useQuery({
    queryKey: ["viewings", params],
    queryFn: () => api.viewings.list(params),
  });
}

export function useUpdateViewingStatus(viewingId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateViewingStatusRequest) => api.viewings.updateStatus(viewingId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["viewings"] }),
  });
}
