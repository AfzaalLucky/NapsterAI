import { zodResolver } from "@hookform/resolvers/zod";
import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon, PlusIcon } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "@/api/client";
import { InventoryStatuses } from "@/api/types";
import type { Inventory } from "@/api/types";
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
import { Textarea } from "@/components/ui/textarea";
import { useDeleteInventoryUnit, useCreateInventoryUnit, useUpdateInventoryUnit } from "@/features/inventory/useInventoryMutations";
import { useInventoryListQuery } from "@/features/inventory/useInventory";
import { useProjectUnitTypesQuery, useProjectsQuery } from "@/features/projects/useProjects";
import { formatPrice } from "@/lib/format";

const unitSchema = z.object({
  projectId: z.number().int().positive("Select a project"),
  unitTypeId: z.number().int().positive("Select a unit type"),
  unitNumber: z.string().min(1, "Unit number is required"),
  bedrooms: z.number().int().nonnegative().optional(),
  areaSqFt: z.number().nonnegative().optional(),
  listPrice: z.number().nonnegative().optional(),
  status: z.enum(InventoryStatuses, { message: "Select a status" }),
  viewType: z.string().optional(),
  furnishingStatus: z.string().optional(),
  notes: z.string().optional(),
});

type UnitFormValues = z.infer<typeof unitSchema>;

const EMPTY_VALUES: UnitFormValues = {
  projectId: 0,
  unitTypeId: 0,
  unitNumber: "",
  status: "Available",
  viewType: "",
  furnishingStatus: "",
  notes: "",
};

export function InventoryAdminPage() {
  const [projectFilter, setProjectFilter] = useState<string>("all");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [dialogUnit, setDialogUnit] = useState<Inventory | "new" | null>(null);

  const { data: projects } = useProjectsQuery({ pageSize: 100 });
  const projectNameById = useMemo(
    () => new Map((projects?.items ?? []).map((p) => [p.projectId, p.projectName])),
    [projects],
  );

  const { data, isPending } = useInventoryListQuery({
    projectId: projectFilter === "all" ? undefined : Number(projectFilter),
    status: statusFilter === "all" ? undefined : statusFilter,
    pageSize: 100,
  });

  const deleteUnit = useDeleteInventoryUnit();

  const columns = useMemo<ColumnDef<Inventory, unknown>[]>(
    () => [
      { accessorKey: "unitNumber", header: "Unit" },
      {
        id: "project",
        header: "Project",
        cell: ({ row }) => projectNameById.get(row.original.projectId) ?? row.original.projectId,
      },
      { accessorKey: "bedrooms", header: "Bed" },
      {
        id: "areaSqFt",
        accessorKey: "areaSqFt",
        header: "Area",
        cell: ({ row }) => (row.original.areaSqFt != null ? `${row.original.areaSqFt} sq ft` : "—"),
      },
      {
        id: "listPrice",
        accessorKey: "listPrice",
        header: "Price",
        cell: ({ row }) => (row.original.listPrice != null ? formatPrice(row.original.listPrice, "AED") : "—"),
      },
      {
        id: "status",
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => <Badge variant="outline">{row.original.status}</Badge>,
      },
      {
        id: "actions",
        header: "",
        enableSorting: false,
        cell: ({ row }) => {
          const unit = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontalIcon className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
                <DropdownMenuItem onClick={() => setDialogUnit(unit)}>Edit</DropdownMenuItem>
                <DropdownMenuItem
                  variant="destructive"
                  onClick={() => {
                    if (confirm(`Delete unit "${unit.unitNumber}"? This cannot be undone.`)) {
                      deleteUnit.mutate(unit.inventoryId, {
                        onSuccess: () => toast.success(`Unit ${unit.unitNumber} deleted.`),
                        onError: (e) =>
                          toast.error(e instanceof ApiError ? e.message : "Could not delete unit."),
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
    [projectNameById, deleteUnit],
  );

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Inventory</h1>
        <Button onClick={() => setDialogUnit("new")}>
          <PlusIcon />
          New Unit
        </Button>
      </div>

      <div className="mt-4 flex gap-2">
        <Select value={projectFilter} onValueChange={setProjectFilter}>
          <SelectTrigger className="w-56">
            <SelectValue placeholder="All projects" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All projects</SelectItem>
            {(projects?.items ?? []).map((p) => (
              <SelectItem key={p.projectId} value={String(p.projectId)}>
                {p.projectName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            {InventoryStatuses.map((status) => (
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
          emptyMessage="No inventory units found."
          getRowId={(row) => String(row.inventoryId)}
        />
      </div>

      <Dialog open={dialogUnit !== null} onOpenChange={(open) => !open && setDialogUnit(null)}>
        {dialogUnit !== null && (
          <UnitFormDialogContent unit={dialogUnit === "new" ? null : dialogUnit} onClose={() => setDialogUnit(null)} />
        )}
      </Dialog>
    </div>
  );
}

function UnitFormDialogContent({ unit, onClose }: { unit: Inventory | null; onClose: () => void }) {
  const isEditing = unit !== null;
  const form = useForm<UnitFormValues>({
    resolver: zodResolver(unitSchema),
    defaultValues: unit
      ? {
          projectId: unit.projectId,
          unitTypeId: unit.unitTypeId,
          unitNumber: unit.unitNumber,
          bedrooms: unit.bedrooms ?? undefined,
          areaSqFt: unit.areaSqFt ?? undefined,
          listPrice: unit.listPrice ?? undefined,
          status: unit.status as UnitFormValues["status"],
          viewType: unit.viewType ?? "",
          furnishingStatus: unit.furnishingStatus ?? "",
          notes: unit.notes ?? "",
        }
      : EMPTY_VALUES,
  });

  const selectedProjectId = form.watch("projectId");
  const { data: projects } = useProjectsQuery({ pageSize: 100 });
  const { data: unitTypes } = useProjectUnitTypesQuery(selectedProjectId);

  // Reset the unit-type selection when switching projects, since unit types are per-project.
  useEffect(() => {
    if (!isEditing) {
      form.setValue("unitTypeId", 0);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedProjectId]);

  const createUnit = useCreateInventoryUnit();
  const updateUnit = useUpdateInventoryUnit(unit?.inventoryId ?? 0);
  const isSaving = createUnit.isPending || updateUnit.isPending;

  function onSubmit(values: UnitFormValues) {
    const body = {
      ...values,
      viewType: values.viewType || undefined,
      furnishingStatus: values.furnishingStatus || undefined,
      notes: values.notes || undefined,
    };

    const handlers = {
      onSuccess: () => {
        toast.success(isEditing ? "Unit updated." : "Unit created.");
        onClose();
      },
      onError: (error: unknown) => {
        toast.error(error instanceof ApiError ? error.message : "Could not save the unit.");
      },
    };

    if (isEditing) {
      updateUnit.mutate({ ...body, soldOrLeasedDate: unit.soldOrLeasedDate, buyerTenantName: unit.buyerTenantName }, handlers);
    } else {
      createUnit.mutate(body, handlers);
    }
  }

  return (
    <DialogContent>
      <DialogHeader>
        <DialogTitle>{isEditing ? `Edit Unit ${unit.unitNumber}` : "New Unit"}</DialogTitle>
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="projectId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Project</FormLabel>
                  <Select value={String(field.value || "")} onValueChange={(v) => field.onChange(Number(v))}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select project" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {(projects?.items ?? []).map((p) => (
                        <SelectItem key={p.projectId} value={String(p.projectId)}>
                          {p.projectName}
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
              name="unitTypeId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Unit Type</FormLabel>
                  <Select value={String(field.value || "")} onValueChange={(v) => field.onChange(Number(v))}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select type" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {(unitTypes ?? []).map((t) => (
                        <SelectItem key={t.unitTypeId} value={String(t.unitTypeId)}>
                          {t.typeName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="unitNumber"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Unit Number</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="status"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Status</FormLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {InventoryStatuses.map((status) => (
                        <SelectItem key={status} value={status}>
                          {status}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="grid grid-cols-3 gap-4">
            <FormField
              control={form.control}
              name="bedrooms"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Bedrooms</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      value={field.value ?? ""}
                      onChange={(e) => field.onChange(e.target.value === "" ? undefined : Number(e.target.value))}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="areaSqFt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Area (sq ft)</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      value={field.value ?? ""}
                      onChange={(e) => field.onChange(e.target.value === "" ? undefined : Number(e.target.value))}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="listPrice"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>List Price</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      value={field.value ?? ""}
                      onChange={(e) => field.onChange(e.target.value === "" ? undefined : Number(e.target.value))}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="viewType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>View Type</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="furnishingStatus"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Furnishing</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <FormField
            control={form.control}
            name="notes"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Notes</FormLabel>
                <FormControl>
                  <Textarea rows={2} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSaving}>
              {isSaving ? "Saving…" : isEditing ? "Save changes" : "Create unit"}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </DialogContent>
  );
}

