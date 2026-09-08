import type { ColumnDef } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { Lead } from "@/api/types";
import { LeadStatuses } from "@/api/types";
import { LeadStatusBadge } from "@/components/admin/LeadStatusBadge";
import { DataTable } from "@/components/admin/DataTable";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useAgentsQuery } from "@/features/agents/useAgents";
import { useLeadsQuery } from "@/features/leads/useLeads";
import { formatPrice } from "@/lib/format";

export function LeadsAdminPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState("all");
  const [agentFilter, setAgentFilter] = useState("all");

  const { data: agents } = useAgentsQuery();
  const { data, isPending } = useLeadsQuery({
    status: statusFilter === "all" ? undefined : statusFilter,
    salesAgentId: agentFilter === "all" ? undefined : Number(agentFilter),
    pageIndex: 0,
    pageSize: 100,
  });

  const columns = useMemo<ColumnDef<Lead, unknown>[]>(
    () => [
      { id: "customerName", header: "Customer", accessorFn: (row) => row.customer.fullName },
      { id: "customerEmail", header: "Email", accessorFn: (row) => row.customer.email },
      {
        id: "status",
        header: "Status",
        cell: ({ row }) => <LeadStatusBadge status={row.original.status} />,
      },
      { accessorKey: "salesAgentName", header: "Agent", cell: ({ row }) => row.original.salesAgentName ?? "Unassigned" },
      {
        id: "budget",
        header: "Budget",
        cell: ({ row }) => (row.original.budget != null ? formatPrice(row.original.budget, "AED") : "—"),
      },
      {
        id: "createdDate",
        header: "Created",
        cell: ({ row }) => new Date(row.original.createdDate).toLocaleDateString(),
      },
    ],
    [],
  );

  return (
    <div>
      <h1 className="text-2xl font-semibold">Leads</h1>

      <div className="mt-4 flex gap-2">
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            {LeadStatuses.map((status) => (
              <SelectItem key={status} value={status}>
                {status}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={agentFilter} onValueChange={setAgentFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All agents" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All agents</SelectItem>
            {(agents ?? []).map((agent) => (
              <SelectItem key={agent.salesAgentId} value={String(agent.salesAgentId)}>
                {agent.fullName}
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
          emptyMessage="No leads found."
          getRowId={(row) => String(row.leadId)}
          onRowClick={(row) => navigate(`/admin/leads/${row.leadId}`)}
        />
      </div>
    </div>
  );
}
