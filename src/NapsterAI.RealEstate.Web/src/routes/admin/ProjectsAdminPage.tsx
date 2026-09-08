import type { ColumnDef, RowSelectionState } from "@tanstack/react-table";
import { MoreHorizontalIcon, PlusIcon } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { Link } from "react-router-dom";
import type { Project } from "@/api/types";
import { ApiError } from "@/api/client";
import { BulkActionsToolbar } from "@/components/admin/BulkActionsToolbar";
import { DataTable } from "@/components/admin/DataTable";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  useApproveProject,
  useDeleteProject,
  useRejectProject,
} from "@/features/projects/useProjectMutations";
import { useProjectsQuery } from "@/features/projects/useProjects";
import { useAuthStore } from "@/lib/auth-store";
import { formatPrice } from "@/lib/format";

const APPROVAL_STYLES: Record<string, string> = {
  Draft: "bg-slate-100 text-slate-700 dark:bg-slate-500/20 dark:text-slate-300",
  PendingReview: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
  Approved: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
  Rejected: "bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300",
};

export function ProjectsAdminPage() {
  const isAdmin = useAuthStore((state) => state.role === "Admin");
  const [search, setSearch] = useState("");
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  const { data, isPending } = useProjectsQuery({ search: search || undefined, pageIndex: 0, pageSize: 100 });

  const approveProject = useApproveProject();
  const rejectProject = useRejectProject();
  const deleteProject = useDeleteProject();

  const selectedIds = Object.keys(rowSelection).map(Number);

  function handleError(error: unknown, fallback: string) {
    toast.error(error instanceof ApiError ? error.message : fallback);
  }

  async function bulkApprove() {
    await Promise.all(
      selectedIds.map((id) => approveProject.mutateAsync(id).catch((e) => handleError(e, "Could not approve a project."))),
    );
    toast.success("Selected projects approved.");
    setRowSelection({});
  }

  async function bulkReject() {
    await Promise.all(
      selectedIds.map((id) => rejectProject.mutateAsync(id).catch((e) => handleError(e, "Could not reject a project."))),
    );
    toast.success("Selected projects rejected.");
    setRowSelection({});
  }

  async function bulkDelete() {
    await Promise.all(
      selectedIds.map((id) => deleteProject.mutateAsync(id).catch((e) => handleError(e, "Could not delete a project."))),
    );
    toast.success("Selected projects deleted.");
    setRowSelection({});
  }

  const columns = useMemo<ColumnDef<Project, unknown>[]>(
    () => [
      { accessorKey: "projectCode", header: "Code" },
      { accessorKey: "projectName", header: "Name" },
      { accessorKey: "city", header: "City" },
      { accessorKey: "projectType", header: "Type" },
      { accessorKey: "status", header: "Status" },
      {
        id: "approvalStatus",
        accessorKey: "approvalStatus",
        header: "Approval",
        cell: ({ row }) => (
          <Badge variant="outline" className={`border-transparent ${APPROVAL_STYLES[row.original.approvalStatus]}`}>
            {row.original.approvalStatus}
          </Badge>
        ),
      },
      {
        id: "startingPrice",
        accessorKey: "startingPrice",
        header: "From",
        cell: ({ row }) =>
          row.original.startingPrice != null ? formatPrice(row.original.startingPrice, row.original.currency) : "—",
      },
      {
        id: "featured",
        header: "Featured",
        cell: ({ row }) => (row.original.isFeatured ? <Badge>Featured</Badge> : null),
        enableSorting: false,
      },
      {
        id: "actions",
        header: "",
        enableSorting: false,
        cell: ({ row }) => {
          const project = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontalIcon className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
                <DropdownMenuItem asChild>
                  <Link to={`/admin/projects/${project.projectId}/edit`}>Edit</Link>
                </DropdownMenuItem>
                {isAdmin && project.approvalStatus !== "Approved" && (
                  <DropdownMenuItem
                    onClick={() =>
                      approveProject.mutate(project.projectId, {
                        onSuccess: () => toast.success(`${project.projectName} approved.`),
                        onError: (e) => handleError(e, "Could not approve project."),
                      })
                    }
                  >
                    Approve
                  </DropdownMenuItem>
                )}
                {isAdmin && project.approvalStatus !== "Rejected" && (
                  <DropdownMenuItem
                    onClick={() =>
                      rejectProject.mutate(project.projectId, {
                        onSuccess: () => toast.success(`${project.projectName} rejected.`),
                        onError: (e) => handleError(e, "Could not reject project."),
                      })
                    }
                  >
                    Reject
                  </DropdownMenuItem>
                )}
                <DropdownMenuItem
                  variant="destructive"
                  onClick={() => {
                    if (confirm(`Delete "${project.projectName}"? This cannot be undone.`)) {
                      deleteProject.mutate(project.projectId, {
                        onSuccess: () => toast.success(`${project.projectName} deleted.`),
                        onError: (e) => handleError(e, "Could not delete project."),
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
    [isAdmin, approveProject, rejectProject, deleteProject],
  );

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Projects</h1>
        <Button asChild>
          <Link to="/admin/projects/new">
            <PlusIcon />
            New Project
          </Link>
        </Button>
      </div>

      <div className="mt-4 flex items-center gap-2">
        <Input
          placeholder="Search by name or code…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-xs"
        />
      </div>

      <div className="mt-4">
        <BulkActionsToolbar selectedCount={selectedIds.length} onClear={() => setRowSelection({})}>
          {isAdmin && (
            <>
              <Button size="sm" variant="outline" onClick={bulkApprove}>
                Approve
              </Button>
              <Button size="sm" variant="outline" onClick={bulkReject}>
                Reject
              </Button>
            </>
          )}
          <Button size="sm" variant="destructive" onClick={bulkDelete}>
            Delete
          </Button>
        </BulkActionsToolbar>

        <DataTable
          columns={columns}
          data={data?.items ?? []}
          isLoading={isPending}
          emptyMessage="No projects found."
          enableRowSelection
          rowSelection={rowSelection}
          onRowSelectionChange={setRowSelection}
          getRowId={(row) => String(row.projectId)}
        />
      </div>
    </div>
  );
}
