import { useMutation } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { CreateInquiryRequest } from "@/api/types";

export function useCreateInquiry() {
  return useMutation({
    mutationFn: (body: CreateInquiryRequest) => api.inquiries.create(body),
  });
}
