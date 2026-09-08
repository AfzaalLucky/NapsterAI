import { useMutation } from "@tanstack/react-query";
import { api } from "@/api/client";
import type { CalculatePaymentPlanRequest } from "@/api/types";

export function useCalculatePaymentPlan(inventoryId: number) {
  return useMutation({
    mutationFn: (body: CalculatePaymentPlanRequest) => api.inventory.calculatePaymentPlan(inventoryId, body),
  });
}
