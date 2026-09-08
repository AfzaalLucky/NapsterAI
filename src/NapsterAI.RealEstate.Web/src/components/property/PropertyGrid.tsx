import { SearchXIcon } from "lucide-react";
import type { Project } from "@/api/types";
import { EmptyState } from "@/components/EmptyState";
import { PropertyCard } from "@/components/property/PropertyCard";
import { Skeleton } from "@/components/ui/skeleton";

export function PropertyGrid({
  projects,
  isLoading,
}: {
  projects: Project[] | undefined;
  isLoading: boolean;
}) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }, (_, i) => (
          <Skeleton key={i} className="aspect-[4/5] w-full" />
        ))}
      </div>
    );
  }

  if (!projects || projects.length === 0) {
    return (
      <EmptyState
        icon={SearchXIcon}
        title="No projects found"
        description="Try adjusting your filters or search terms."
      />
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {projects.map((project) => (
        <PropertyCard key={project.projectId} project={project} />
      ))}
    </div>
  );
}
