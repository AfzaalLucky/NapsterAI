import type { ColumnDef } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { ApiError } from "@/api/client";
import type { Viewing } from "@/api/types";
import { ViewingStatuses } from "@/api/types";
import { DataTable } from "@/components/admin/DataTable";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useInventoryListQuery } from "@/features/inventory/useInventory";
import { useUpdateViewingStatus, useViewingsQuery } from "@/features/viewings/useViewings";

export function ViewingsAdminPage() {
  const [statusFilter, setStatusFilter] = useState("all");

  const { data, isPending } = useViewingsQuery({
    status: statusFilter === "all" ? undefined : statusFilter,
    pageIndex: 0,
    pageSize: 100,
  });
  const { data: inventory } = useInventoryListQuery({ pageSize: 100 });
  const unitNumberById = useMemo(
    () => new Map((inventory?.items ?? []).map((u) => [u.inventoryId, u.unitNumber])),
    [inventory],
  );

  const columns = useMemo<ColumnDef<Viewing, unknown>[]>(
    () => [
      { id: "leadId", header: "Lead #", accessorKey: "leadId" },
      {
        id: "unit",
        header: "Unit",
        cell: ({ row }) => unitNumberById.get(row.original.inventoryId) ?? row.original.inventoryId,
      },
      {
        id: "scheduledDate",
        header: "Scheduled",
        cell: ({ row }) => new Date(row.original.scheduledDate).toLocaleString(),
      },
      {
        id: "status",
        header: "Status",
        cell: ({ row }) => <ViewingStatusCell viewing={row.original} />,
      },
      { accessorKey: "notes", header: "Notes" },
    ],
    [unitNumberById],
  );

  return (
    <div>
      <h1 className="text-2xl font-semibold">Viewings</h1>

      <div className="mt-4">
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            {ViewingStatuses.map((status) => (
              <SelectItem key={status} value={status}>
                {status}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="mt-4">
        <DataTable
          columns={columns}
          data={data?.items ?? []}
          isLoading={isPending}
          emptyMessage="No viewings found."
          getRowId={(row) => String(row.viewingId)}
        />
      </div>
    </div>
  );
}

function ViewingStatusCell({ viewing }: { viewing: Viewing }) {
  const updateStatus = useUpdateViewingStatus(viewing.viewingId);

  return (
    <Select
      value={viewing.status}
      onValueChange={(status) =>
        updateStatus.mutate(
          { status },
          {
            onSuccess: () => toast.success("Viewing status updated."),
            onError: (e) => toast.error(e instanceof ApiError ? e.message : "Could not update viewing."),
          },
        )
      }
    >
      <SelectTrigger className="w-40" onClick={(e) => e.stopPropagation()}>
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {ViewingStatuses.map((status) => (
          <SelectItem key={status} value={status}>
            {status}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
