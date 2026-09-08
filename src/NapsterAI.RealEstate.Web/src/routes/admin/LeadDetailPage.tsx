import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Link, useParams } from "react-router-dom";
import { z } from "zod";
import { ApiError } from "@/api/client";
import { LeadActivityTypes, LeadStatuses } from "@/api/types";
import { LeadStatusBadge } from "@/components/admin/LeadStatusBadge";
import { BookViewingForm } from "@/components/property/BookViewingForm";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Form, FormControl, FormField, FormItem, FormMessage } from "@/components/ui/form";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { useAgentsQuery } from "@/features/agents/useAgents";
import {
  useAddLeadActivity,
  useAssignLead,
  useLeadActivitiesQuery,
  useLeadQuery,
  useUpdateLeadStatus,
} from "@/features/leads/useLeads";
import { formatPrice } from "@/lib/format";

const activitySchema = z.object({
  activityType: z.enum(LeadActivityTypes, { message: "Select an activity type" }),
  notes: z.string().optional(),
});
type ActivityFormValues = z.infer<typeof activitySchema>;

export function LeadDetailPage() {
  const { leadId: leadIdParam } = useParams<{ leadId: string }>();
  const leadId = Number(leadIdParam);

  const { data: lead, isPending } = useLeadQuery(leadId);
  const { data: activities } = useLeadActivitiesQuery(leadId);
  const { data: agents } = useAgentsQuery();

  const updateStatus = useUpdateLeadStatus(leadId);
  const assignLead = useAssignLead(leadId);
  const addActivity = useAddLeadActivity(leadId);

  const activityForm = useForm<ActivityFormValues>({
    resolver: zodResolver(activitySchema),
    defaultValues: { activityType: "Note", notes: "" },
  });

  if (isPending) {
    return <Skeleton className="h-64 w-full" />;
  }

  if (!lead) {
    return <p className="text-muted-foreground">Lead not found.</p>;
  }

  function onAddActivity(values: ActivityFormValues) {
    addActivity.mutate(values, {
      onSuccess: () => {
        toast.success("Activity logged.");
        activityForm.reset({ activityType: "Note", notes: "" });
      },
      onError: (e) => toast.error(e instanceof ApiError ? e.message : "Could not log activity."),
    });
  }

  return (
    <div>
      <div className="text-muted-foreground mb-4 text-sm">
        <Link to="/admin/leads" className="hover:text-foreground">
          Leads
        </Link>
        {" / "}
        <span className="text-foreground">{lead.customer.fullName}</span>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-2xl font-semibold">{lead.customer.fullName}</h1>
        <LeadStatusBadge status={lead.status} />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-[1fr_20rem]">
        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Activity Timeline</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-4">
              <Form {...activityForm}>
                <form onSubmit={activityForm.handleSubmit(onAddActivity)} className="flex flex-col gap-3 border-b pb-4">
                  <div className="flex gap-2">
                    <FormField
                      control={activityForm.control}
                      name="activityType"
                      render={({ field }) => (
                        <FormItem className="w-40">
                          <Select value={field.value} onValueChange={field.onChange}>
                            <FormControl>
                              <SelectTrigger className="w-full">
                                <SelectValue />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              {LeadActivityTypes.filter((t) => t !== "StatusChange" && t !== "ViewingBooked").map(
                                (type) => (
                                  <SelectItem key={type} value={type}>
                                    {type}
                                  </SelectItem>
                                ),
                              )}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <Button type="submit" disabled={addActivity.isPending} className="shrink-0">
                      {addActivity.isPending ? "Adding…" : "Log activity"}
                    </Button>
                  </div>
                  <FormField
                    control={activityForm.control}
                    name="notes"
                    render={({ field }) => (
                      <FormItem>
                        <FormControl>
                          <Textarea rows={2} placeholder="Notes…" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </form>
              </Form>

              {!activities || activities.length === 0 ? (
                <p className="text-muted-foreground text-sm">No activity yet.</p>
              ) : (
                <ul className="flex flex-col gap-3">
                  {activities.map((activity) => (
                    <li key={activity.leadActivityId} className="flex gap-3 text-sm">
                      <Badge variant="outline" className="shrink-0">
                        {activity.activityType}
                      </Badge>
                      <div>
                        <p>{activity.notes}</p>
                        <p className="text-muted-foreground text-xs">
                          {new Date(activity.createdDate).toLocaleString()}
                          {activity.createdByUserEmail ? ` · ${activity.createdByUserEmail}` : ""}
                        </p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Details</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-3 text-sm">
              <Row label="Email" value={lead.customer.email} />
              <Row label="Phone" value={lead.customer.phone ?? "—"} />
              <Row label="Source" value={lead.source ?? "—"} />
              <Row label="Budget" value={lead.budget != null ? formatPrice(lead.budget, "AED") : "—"} />
              {lead.requirementsNotes && <Row label="Requirements" value={lead.requirementsNotes} />}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Status</CardTitle>
            </CardHeader>
            <CardContent>
              <Select
                value={lead.status}
                onValueChange={(status) =>
                  updateStatus.mutate(
                    { status },
                    {
                      onSuccess: () => toast.success("Status updated."),
                      onError: (e) => toast.error(e instanceof ApiError ? e.message : "Could not update status."),
                    },
                  )
                }
              >
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LeadStatuses.map((status) => (
                    <SelectItem key={status} value={status}>
                      {status}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Assigned Agent</CardTitle>
            </CardHeader>
            <CardContent>
              <Select
                value={lead.salesAgentId ? String(lead.salesAgentId) : ""}
                onValueChange={(agentId) =>
                  assignLead.mutate(
                    { salesAgentId: Number(agentId) },
                    {
                      onSuccess: () => toast.success("Lead assigned."),
                      onError: (e) => toast.error(e instanceof ApiError ? e.message : "Could not assign lead."),
                    },
                  )
                }
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Unassigned" />
                </SelectTrigger>
                <SelectContent>
                  {(agents ?? []).map((agent) => (
                    <SelectItem key={agent.salesAgentId} value={String(agent.salesAgentId)}>
                      {agent.fullName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </CardContent>
          </Card>

          {lead.inventoryId != null && (
            <Dialog>
              <DialogTrigger asChild>
                <Button variant="outline">Schedule Viewing</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Schedule a Viewing</DialogTitle>
                </DialogHeader>
                <BookViewingForm inventoryId={lead.inventoryId} />
              </DialogContent>
            </Dialog>
          )}
        </div>
      </div>
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4">
      <span className="text-muted-foreground">{label}</span>
      <span className="text-right">{value}</span>
    </div>
  );
}
