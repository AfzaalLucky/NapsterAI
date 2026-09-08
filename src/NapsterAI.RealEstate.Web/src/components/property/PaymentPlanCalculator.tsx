import { useState } from "react";
import { ApiError } from "@/api/client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { useCalculatePaymentPlan } from "@/features/payment-plan/useCalculatePaymentPlan";
import { formatPrice } from "@/lib/format";

/** Backs the EdgeMCP "calculatePaymentPlan" tool's public UI counterpart - anonymous, read-only. */
export function PaymentPlanCalculator({ inventoryId }: { inventoryId: number }) {
  const [downPaymentPercent, setDownPaymentPercent] = useState(10);
  const calculate = useCalculatePaymentPlan(inventoryId);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-end gap-3">
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="down-payment">Down payment %</Label>
          <Input
            id="down-payment"
            type="number"
            min={0}
            max={100}
            value={downPaymentPercent}
            onChange={(e) => setDownPaymentPercent(Number(e.target.value))}
            className="w-28"
          />
        </div>
        <Button
          onClick={() => calculate.mutate({ downPaymentPercent })}
          disabled={calculate.isPending}
        >
          {calculate.isPending ? "Calculating…" : "Calculate"}
        </Button>
      </div>

      {calculate.isPending && <Skeleton className="h-32 w-full" />}

      {calculate.isError && (
        <p className="text-destructive text-sm">
          {calculate.error instanceof ApiError
            ? calculate.error.message
            : "Could not calculate a payment plan for this unit."}
        </p>
      )}

      {calculate.data && (
        <div className="overflow-x-auto rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="p-2 font-medium">Milestone</th>
                <th className="p-2 font-medium">%</th>
                <th className="p-2 font-medium">Amount</th>
                <th className="p-2 font-medium">Trigger</th>
              </tr>
            </thead>
            <tbody>
              {calculate.data.milestones.map((item, index) => (
                <tr key={`${item.milestoneName}-${index}`} className="border-t">
                  <td className="p-2">{item.milestoneName}</td>
                  <td className="p-2">{item.percentDue.toFixed(1)}%</td>
                  <td className="p-2">{formatPrice(item.amountDue, calculate.data.currency)}</td>
                  <td className="p-2">{item.triggerEvent ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
