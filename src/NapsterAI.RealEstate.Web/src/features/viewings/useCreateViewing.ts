import { useMutation } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { CreateViewingRequest } from "@/api/types";

export function useCreateViewing() {
  return useMutation({
    mutationFn: (body: CreateViewingRequest) => api.viewings.create(body),
  });
}
