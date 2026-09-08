import { zodResolver } from "@hookform/resolvers/zod";
import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon, PlusIcon } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "@/api/client";
import type { Organization } from "@/api/types";
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
import { Switch } from "@/components/ui/switch";
import {
  useCreateOrganization,
  useDeleteOrganization,
  useOrganizationsQuery,
  useUpdateOrganization,
} from "@/features/organizations/useOrganizations";

const organizationSchema = z.object({
  name: z.string().min(1, "Name is required"),
  licenseNumber: z.string().optional(),
  logoUrl: z.string().optional(),
  phone: z.string().optional(),
  email: z.union([z.email(), z.literal("")]).optional(),
  website: z.string().optional(),
  isActive: z.boolean(),
});

type OrganizationFormValues = z.infer<typeof organizationSchema>;

const EMPTY_VALUES: OrganizationFormValues = {
  name: "",
  licenseNumber: "",
  logoUrl: "",
  phone: "",
  email: "",
  website: "",
  isActive: true,
};

export function OrganizationsAdminPage() {
  const [dialogOrg, setDialogOrg] = useState<Organization | "new" | null>(null);
  const { data: organizations, isPending } = useOrganizationsQuery();
  const deleteOrganization = useDeleteOrganization();

  const columns = useMemo<ColumnDef<Organization, unknown>[]>(
    () => [
      { accessorKey: "name", header: "Name" },
      { accessorKey: "licenseNumber", header: "License #" },
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
          const org = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontalIcon className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
                <DropdownMenuItem onClick={() => setDialogOrg(org)}>Edit</DropdownMenuItem>
                <DropdownMenuItem
                  variant="destructive"
                  onClick={() => {
                    if (confirm(`Delete "${org.name}"? This cannot be undone.`)) {
                      deleteOrganization.mutate(org.organizationId, {
                        onSuccess: () => toast.success(`${org.name} deleted.`),
                        onError: (e) =>
                          toast.error(
                            e instanceof ApiError
                              ? e.message
                              : "Could not delete organization.",
                          ),
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
    [deleteOrganization],
  );

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Organizations</h1>
        <Button onClick={() => setDialogOrg("new")}>
          <PlusIcon />
          New Organization
        </Button>
      </div>

      <div className="mt-4">
        <DataTable
          columns={columns}
          data={organizations ?? []}
          isLoading={isPending}
          emptyMessage="No organizations found."
          getRowId={(row) => String(row.organizationId)}
        />
      </div>

      <Dialog open={dialogOrg !== null} onOpenChange={(open) => !open && setDialogOrg(null)}>
        {dialogOrg !== null && (
          <OrganizationFormDialogContent
            organization={dialogOrg === "new" ? null : dialogOrg}
            onClose={() => setDialogOrg(null)}
          />
        )}
      </Dialog>
    </div>
  );
}

function OrganizationFormDialogContent({
  organization,
  onClose,
}: {
  organization: Organization | null;
  onClose: () => void;
}) {
  const isEditing = organization !== null;
  const form = useForm<OrganizationFormValues>({
    resolver: zodResolver(organizationSchema),
    defaultValues: organization
      ? {
          name: organization.name,
          licenseNumber: organization.licenseNumber ?? "",
          logoUrl: organization.logoUrl ?? "",
          phone: organization.phone ?? "",
          email: organization.email ?? "",
          website: organization.website ?? "",
          isActive: organization.isActive,
        }
      : EMPTY_VALUES,
  });

  const createOrganization = useCreateOrganization();
  const updateOrganization = useUpdateOrganization(organization?.organizationId ?? 0);
  const isSaving = createOrganization.isPending || updateOrganization.isPending;

  function onSubmit(values: OrganizationFormValues) {
    const body = {
      ...values,
      licenseNumber: values.licenseNumber || undefined,
      logoUrl: values.logoUrl || undefined,
      phone: values.phone || undefined,
      email: values.email || undefined,
      website: values.website || undefined,
    };

    const handlers = {
      onSuccess: () => {
        toast.success(isEditing ? "Organization updated." : "Organization created.");
        onClose();
      },
      onError: (error: unknown) => {
        toast.error(error instanceof ApiError ? error.message : "Could not save the organization.");
      },
    };

    if (isEditing) {
      updateOrganization.mutate(body, handlers);
    } else {
      createOrganization.mutate(body, handlers);
    }
  }

  return (
    <DialogContent>
      <DialogHeader>
        <DialogTitle>{isEditing ? `Edit ${organization.name}` : "New Organization"}</DialogTitle>
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Name</FormLabel>
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
          </div>
          <div className="grid grid-cols-2 gap-4">
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
            <FormField
              control={form.control}
              name="website"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Website</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

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
              {isSaving ? "Saving…" : isEditing ? "Save changes" : "Create organization"}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </DialogContent>
  );
}
