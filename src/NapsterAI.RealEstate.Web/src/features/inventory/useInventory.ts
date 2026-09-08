import { useQuery } from "@tanstack/react-query";
import { api } from "@/api/client";

export interface InventorySearchParams {
  projectId?: number;
  unitTypeId?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minPrice?: number;
  maxPrice?: number;
  status?: string;
  viewType?: string;
  sortBy?: string;
  sortDescending?: boolean;
  pageIndex?: number;
  pageSize?: number;
}

export function useInventoryListQuery(params: InventorySearchParams) {
  return useQuery({
    queryKey: ["inventory", params],
    queryFn: () => api.inventory.list(params),
  });
}

export function useInventoryQuery(inventoryId: number) {
  return useQuery({
    queryKey: ["inventory", inventoryId],
    queryFn: () => api.inventory.get(inventoryId),
    enabled: Number.isFinite(inventoryId),
  });
}
