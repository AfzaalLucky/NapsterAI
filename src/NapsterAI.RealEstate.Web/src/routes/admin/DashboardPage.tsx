import { useQuery } from "@tanstack/react-query";
import { BarChart3Icon, BuildingIcon, CalendarClockIcon, TrendingUpIcon, UsersIcon, WalletIcon } from "lucide-react";
import { Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { api } from "@/api/client";
import { KpiCard } from "@/components/admin/KpiCard";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { formatNumber, formatPrice } from "@/lib/format";

const LEAD_STATUS_COLORS: Record<string, string> = {
  New: "#94a3b8",
  Contacted: "#60a5fa",
  Qualified: "#818cf8",
  ViewingScheduled: "#c084fc",
  Negotiation: "#fbbf24",
  Won: "#34d399",
  Lost: "#fb7185",
};

const INVENTORY_STATUS_COLORS: Record<string, string> = {
  Available: "#34d399",
  Reserved: "#fbbf24",
  Sold: "#60a5fa",
  Blocked: "#94a3b8",
  Leased: "#818cf8",
};

export function DashboardPage() {
  const { data: leadsSummary, isPending: leadsPending } = useQuery({
    queryKey: ["analytics", "leads-summary"],
    queryFn: () => api.analytics.leadsSummary(),
  });
  const { data: inventorySummary, isPending: inventoryPending } = useQuery({
    queryKey: ["analytics", "inventory-summary"],
    queryFn: () => api.analytics.inventorySummary(),
  });

  const leadsByStatus = Object.entries(leadsSummary?.byStatus ?? {}).map(([status, count]) => ({
    status,
    count,
  }));
  const inventoryByStatus = Object.entries(inventorySummary?.byStatus ?? {}).map(([status, count]) => ({
    status,
    count,
  }));

  return (
    <div>
      <h1 className="text-2xl font-semibold">Dashboard</h1>

      <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <KpiCard label="Total Leads" value={leadsSummary?.totalLeads ?? "—"} icon={UsersIcon} />
        <KpiCard label="Viewings This Week" value={leadsSummary?.viewingsThisWeek ?? "—"} icon={CalendarClockIcon} />
        <KpiCard
          label="Conversion Rate"
          value={leadsSummary?.conversionRatePercent != null ? `${leadsSummary.conversionRatePercent}%` : "—"}
          icon={TrendingUpIcon}
        />
        <KpiCard
          label="Pipeline Value"
          value={leadsSummary ? formatPrice(leadsSummary.pipelineValue, "AED") : "—"}
          icon={WalletIcon}
        />
        <KpiCard label="Total Projects" value={inventorySummary?.totalProjects ?? "—"} icon={BuildingIcon} />
        <KpiCard label="Total Units" value={inventorySummary?.totalUnits ?? "—"} icon={BarChart3Icon} />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Leads by Status</CardTitle>
          </CardHeader>
          <CardContent>
            {leadsPending ? (
              <Skeleton className="h-64 w-full" />
            ) : leadsByStatus.length === 0 ? (
              <p className="text-muted-foreground py-16 text-center text-sm">No leads yet.</p>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={leadsByStatus}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="status" tick={{ fontSize: 11 }} interval={0} angle={-20} textAnchor="end" height={60} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                  <Tooltip formatter={(value) => formatNumber(Number(value))} />
                  <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                    {leadsByStatus.map((entry) => (
                      <Cell key={entry.status} fill={LEAD_STATUS_COLORS[entry.status] ?? "#94a3b8"} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Inventory by Status</CardTitle>
          </CardHeader>
          <CardContent>
            {inventoryPending ? (
              <Skeleton className="h-64 w-full" />
            ) : inventoryByStatus.length === 0 ? (
              <p className="text-muted-foreground py-16 text-center text-sm">No inventory yet.</p>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <PieChart>
                  <Pie data={inventoryByStatus} dataKey="count" nameKey="status" innerRadius={50} outerRadius={90}>
                    {inventoryByStatus.map((entry) => (
                      <Cell key={entry.status} fill={INVENTORY_STATUS_COLORS[entry.status] ?? "#94a3b8"} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => formatNumber(Number(value))} />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
