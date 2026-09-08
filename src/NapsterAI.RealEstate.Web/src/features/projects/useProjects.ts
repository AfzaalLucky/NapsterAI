import { useQuery } from "@tanstack/react-query";
import { api } from "@/api/client";

export interface ProjectSearchParams {
  city?: string;
  projectType?: string;
  status?: string;
  isFeatured?: boolean;
  minPrice?: number;
  maxPrice?: number;
  search?: string;
  pageIndex?: number;
  pageSize?: number;
}

export function useProjectsQuery(params: ProjectSearchParams) {
  return useQuery({
    queryKey: ["projects", params],
    queryFn: () => api.projects.list(params),
  });
}

export function useProjectQuery(projectId: number) {
  return useQuery({
    queryKey: ["projects", projectId],
    queryFn: () => api.projects.get(projectId),
    enabled: Number.isFinite(projectId),
  });
}

export function useProjectAmenitiesQuery(projectId: number) {
  return useQuery({
    queryKey: ["projects", projectId, "amenities"],
    queryFn: () => api.projects.amenities(projectId),
    enabled: Number.isFinite(projectId),
  });
}

export function useProjectMediaQuery(projectId: number) {
  return useQuery({
    queryKey: ["projects", projectId, "media"],
    queryFn: () => api.projects.media(projectId),
    enabled: Number.isFinite(projectId),
  });
}

export function useProjectUnitTypesQuery(projectId: number) {
  return useQuery({
    queryKey: ["projects", projectId, "unit-types"],
    queryFn: () => api.projects.unitTypes(projectId),
    enabled: Number.isFinite(projectId),
  });
}
