import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { CreateInventoryRequest, UpdateInventoryRequest } from "@/api/types";

export function useCreateInventoryUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateInventoryRequest) => api.inventory.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inventory"] }),
  });
}

export function useUpdateInventoryUnit(inventoryId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateInventoryRequest) => api.inventory.update(inventoryId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inventory"] }),
  });
}

export function useDeleteInventoryUnit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (inventoryId: number) => api.inventory.delete(inventoryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inventory"] }),
  });
}
