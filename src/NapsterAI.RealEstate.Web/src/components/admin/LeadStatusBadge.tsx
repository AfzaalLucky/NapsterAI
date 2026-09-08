import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

const STATUS_STYLES: Record<string, string> = {
  New: "bg-slate-100 text-slate-700 dark:bg-slate-500/20 dark:text-slate-300",
  Contacted: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300",
  Qualified: "bg-indigo-100 text-indigo-700 dark:bg-indigo-500/20 dark:text-indigo-300",
  ViewingScheduled: "bg-purple-100 text-purple-700 dark:bg-purple-500/20 dark:text-purple-300",
  Negotiation: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
  Won: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
  Lost: "bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300",
};

export function LeadStatusBadge({ status }: { status: string }) {
  return (
    <Badge variant="outline" className={cn("border-transparent", STATUS_STYLES[status])}>
      {status}
    </Badge>
  );
}
