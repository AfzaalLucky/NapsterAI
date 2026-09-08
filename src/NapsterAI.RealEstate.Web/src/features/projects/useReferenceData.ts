import { useQuery } from "@tanstack/react-query";
import { api } from "@/api/client";

/** Powers PropertyFilters' city/project-type dropdowns. Cached longer - reference data rarely changes. */
export function useLocationsQuery() {
  return useQuery({
    queryKey: ["locations"],
    queryFn: () => api.locations.list({}),
    staleTime: 5 * 60_000,
  });
}

export function useLookupsQuery(type: string) {
  return useQuery({
    queryKey: ["lookups", type],
    queryFn: () => api.lookups.list({ type }),
    staleTime: 5 * 60_000,
  });
}
