import { MapPinIcon } from "lucide-react";
import { Link } from "react-router-dom";
import type { Project } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { formatPrice } from "@/lib/format";

export function PropertyCard({ project }: { project: Project }) {
  return (
    <Link to={`/projects/${project.projectId}`} className="block h-full">
      <Card className="h-full overflow-hidden py-0 transition-shadow hover:shadow-md">
        <div className="bg-muted aspect-video w-full overflow-hidden">
          {project.imageUrl ? (
            <img
              src={project.imageUrl}
              alt={project.projectName}
              className="h-full w-full object-cover"
              loading="lazy"
            />
          ) : (
            <div className="text-muted-foreground flex h-full w-full items-center justify-center text-sm">
              No image
            </div>
          )}
        </div>

        <CardContent className="flex flex-col gap-2 py-4">
          <div className="flex items-start justify-between gap-2">
            <h3 className="font-semibold">{project.projectName}</h3>
            {project.isFeatured && <Badge>Featured</Badge>}
          </div>

          <p className="text-muted-foreground flex items-center gap-1 text-sm">
            <MapPinIcon className="size-3.5" />
            {project.city}, {project.country}
          </p>

          <div className="mt-1 flex items-center justify-between">
            <Badge variant="outline">{project.projectType}</Badge>
            {project.startingPrice != null && (
              <span className="text-sm font-medium">
                From {formatPrice(project.startingPrice, project.currency)}
              </span>
            )}
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
