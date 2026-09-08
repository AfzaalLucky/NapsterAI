import { zodResolver } from "@hookform/resolvers/zod";
import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon, PlusIcon } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "@/api/client";
import type { RealEstateAgent } from "@/api/types";
import { DataTable } from "@/components/admin/DataTable";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { useAgentsQuery, useCreateAgent, useDeleteAgent, useUpdateAgent } from "@/features/agents/useAgents";
import { useOrganizationsQuery } from "@/features/organizations/useOrganizations";

const agentSchema = z.object({
  organizationId: z.number().int().positive("Select an organization"),
  fullName: z.string().min(1, "Full name is required"),
  phone: z.string().optional(),
  email: z.union([z.email(), z.literal("")]).optional(),
  photoUrl: z.string().optional(),
  licenseNumber: z.string().optional(),
  isActive: z.boolean(),
});

type AgentFormValues = z.infer<typeof agentSchema>;

const EMPTY_VALUES: AgentFormValues = {
  organizationId: 0,
  fullName: "",
  phone: "",
  email: "",
  photoUrl: "",
  licenseNumber: "",
  isActive: true,
};

export function AgentsAdminPage() {
  const [dialogAgent, setDialogAgent] = useState<RealEstateAgent | "new" | null>(null);
  const { data: agents, isPending } = useAgentsQuery();
  const deleteAgent = useDeleteAgent();

  const columns = useMemo<ColumnDef<RealEstateAgent, unknown>[]>(
    () => [
      { accessorKey: "fullName", header: "Name" },
      { accessorKey: "organizationName", header: "Organization" },
      { accessorKey: "email", header: "Email" },
      { accessorKey: "phone", header: "Phone" },
      {
        id: "isActive",
        header: "Status",
        cell: ({ row }) => (
          <Badge variant={row.original.isActive ? "default" : "secondary"}>
            {row.original.isActive ? "Active" : "Inactive"}
          </Badge>
        ),
      },
      {
        id: "actions",
        header: "",
        enableSorting: false,
        cell: ({ row }) => {
          const agent = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontalIcon className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
                <DropdownMenuItem onClick={() => setDialogAgent(agent)}>Edit</DropdownMenuItem>
                <DropdownMenuItem
                  variant="destructive"
                  onClick={() => {
                    if (confirm(`Delete "${agent.fullName}"? This cannot be undone.`)) {
                      deleteAgent.mutate(agent.salesAgentId, {
                        onSuccess: () => toast.success(`${agent.fullName} deleted.`),
                        onError: (e) =>
                          toast.error(e instanceof ApiError ? e.message : "Could not delete agent."),
                      });
                    }
                  }}
                >
                  Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
      },
    ],
    [deleteAgent],
  );

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Sales Agents</h1>
        <Button onClick={() => setDialogAgent("new")}>
          <PlusIcon />
          New Agent
        </Button>
      </div>

      <div className="mt-4">
        <DataTable
          columns={columns}
          data={agents ?? []}
          isLoading={isPending}
          emptyMessage="No sales agents found."
          getRowId={(row) => String(row.salesAgentId)}
        />
      </div>

      <Dialog open={dialogAgent !== null} onOpenChange={(open) => !open && setDialogAgent(null)}>
        {dialogAgent !== null && (
          <AgentFormDialogContent agent={dialogAgent === "new" ? null : dialogAgent} onClose={() => setDialogAgent(null)} />
        )}
      </Dialog>
    </div>
  );
}

function AgentFormDialogContent({ agent, onClose }: { agent: RealEstateAgent | null; onClose: () => void }) {
  const isEditing = agent !== null;
  const { data: organizations } = useOrganizationsQuery();

  const form = useForm<AgentFormValues>({
    resolver: zodResolver(agentSchema),
    defaultValues: agent
      ? {
          organizationId: agent.organizationId,
          fullName: agent.fullName,
          phone: agent.phone ?? "",
          email: agent.email ?? "",
          photoUrl: agent.photoUrl ?? "",
          licenseNumber: agent.licenseNumber ?? "",
          isActive: agent.isActive,
        }
      : EMPTY_VALUES,
  });

  const createAgent = useCreateAgent();
  const updateAgent = useUpdateAgent(agent?.salesAgentId ?? 0);
  const isSaving = createAgent.isPending || updateAgent.isPending;

  function onSubmit(values: AgentFormValues) {
    const body = {
      ...values,
      phone: values.phone || undefined,
      email: values.email || undefined,
      photoUrl: values.photoUrl || undefined,
      licenseNumber: values.licenseNumber || undefined,
    };

    const handlers = {
      onSuccess: () => {
        toast.success(isEditing ? "Agent updated." : "Agent created.");
        onClose();
      },
      onError: (error: unknown) => {
        toast.error(error instanceof ApiError ? error.message : "Could not save the agent.");
      },
    };

    if (isEditing) {
      updateAgent.mutate(body, handlers);
    } else {
      createAgent.mutate(body, handlers);
    }
  }

  return (
    <DialogContent>
      <DialogHeader>
        <DialogTitle>{isEditing ? `Edit ${agent.fullName}` : "New Agent"}</DialogTitle>
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <FormField
            control={form.control}
            name="organizationId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Organization</FormLabel>
                <Select value={String(field.value || "")} onValueChange={(v) => field.onChange(Number(v))}>
                  <FormControl>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Select organization" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {(organizations ?? []).map((org) => (
                      <SelectItem key={org.organizationId} value={String(org.organizationId)}>
                        {org.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="fullName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Full Name</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="phone"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
          <FormField
            control={form.control}
            name="licenseNumber"
            render={({ field }) => (
              <FormItem>
                <FormLabel>License #</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {isEditing && (
            <FormField
              control={form.control}
              name="isActive"
              render={({ field }) => (
                <FormItem className="flex flex-row items-center gap-2">
                  <FormControl>
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  </FormControl>
                  <FormLabel className="!mt-0">Active</FormLabel>
                </FormItem>
              )}
            />
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSaving}>
              {isSaving ? "Saving…" : isEditing ? "Save changes" : "Create agent"}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </DialogContent>
  );
}
