import { ChevronRightIcon } from "lucide-react";
import { Helmet } from "react-helmet-async";
import { Link, useParams } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/EmptyState";
import { InquiryForm } from "@/components/property/InquiryForm";
import { useInventoryListQuery } from "@/features/inventory/useInventory";
import {
  useProjectAmenitiesQuery,
  useProjectMediaQuery,
  useProjectQuery,
} from "@/features/projects/useProjects";
import { formatPrice } from "@/lib/format";

export function ProjectDetailPage() {
  const { projectId: projectIdParam } = useParams<{ projectId: string }>();
  const projectId = Number(projectIdParam);

  const { data: project, isPending } = useProjectQuery(projectId);
  const { data: amenities } = useProjectAmenitiesQuery(projectId);
  const { data: media } = useProjectMediaQuery(projectId);
  const { data: units, isPending: unitsPending } = useInventoryListQuery({
    projectId,
    status: "Available",
    pageSize: 50,
  });

  if (isPending) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="mt-4 h-64 w-full" />
      </div>
    );
  }

  if (!project) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <EmptyState title="Project not found" description="This listing may have been removed." />
      </div>
    );
  }

  const galleryImages =
    media && media.length > 0
      ? media.filter((m) => m.mediaType === "Image").map((m) => m.url)
      : project.imageUrl
        ? [project.imageUrl]
        : [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <Helmet>
        <title>{project.projectName} | NapsterAI Real Estate</title>
        <meta name="description" content={project.description?.slice(0, 160) ?? project.projectName} />
      </Helmet>

      <nav className="text-muted-foreground mb-4 flex items-center gap-1 text-sm">
        <Link to="/search" className="hover:text-foreground">
          Search
        </Link>
        <ChevronRightIcon className="size-3.5" />
        <span className="text-foreground">{project.projectName}</span>
      </nav>

      {galleryImages.length > 0 && (
        <div className="mb-6 grid grid-cols-1 gap-2 sm:grid-cols-3">
          {galleryImages.slice(0, 3).map((url, i) => (
            <img
              key={url}
              src={url}
              alt={`${project.projectName} ${i + 1}`}
              className={`bg-muted h-64 w-full rounded-lg object-cover ${i === 0 ? "sm:col-span-2 sm:row-span-2 sm:h-full" : ""}`}
              loading="lazy"
            />
          ))}
        </div>
      )}

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-[1fr_20rem]">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="text-2xl font-semibold">{project.projectName}</h1>
            {project.isFeatured && <Badge>Featured</Badge>}
            <Badge variant="outline">{project.status}</Badge>
          </div>
          <p className="text-muted-foreground mt-1">
            {project.district ? `${project.district}, ` : ""}
            {project.city}, {project.country}
            {project.developer ? ` · by ${project.developer}` : ""}
          </p>

          {project.startingPrice != null && (
            <p className="mt-4 text-lg font-semibold">
              Starting from {formatPrice(project.startingPrice, project.currency)}
            </p>
          )}

          {project.description && <p className="mt-6 whitespace-pre-line text-sm leading-relaxed">{project.description}</p>}

          {amenities && amenities.length > 0 && (
            <div className="mt-8">
              <h2 className="text-lg font-semibold">Amenities</h2>
              <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3">
                {amenities.map((amenity) => (
                  <div key={amenity.amenityId} className="rounded-md border p-3 text-sm">
                    {amenity.amenityName}
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="mt-8">
            <h2 className="text-lg font-semibold">Available Units</h2>
            {unitsPending ? (
              <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
                {Array.from({ length: 4 }, (_, i) => (
                  <Skeleton key={i} className="h-24 w-full" />
                ))}
              </div>
            ) : units && units.items.length > 0 ? (
              <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
                {units.items.map((unit) => (
                  <Link key={unit.inventoryId} to={`/inventory/${unit.inventoryId}`}>
                    <Card className="h-full py-4 transition-shadow hover:shadow-md">
                      <CardContent className="flex items-center justify-between">
                        <div>
                          <p className="font-medium">Unit {unit.unitNumber}</p>
                          <p className="text-muted-foreground text-sm">
                            {unit.bedrooms ?? "—"} bed · {unit.areaSqFt ?? "—"} sq ft
                          </p>
                        </div>
                        {unit.listPrice != null && (
                          <p className="text-sm font-semibold">{formatPrice(unit.listPrice, project.currency)}</p>
                        )}
                      </CardContent>
                    </Card>
                  </Link>
                ))}
              </div>
            ) : (
              <EmptyState title="No available units" description="Check back soon or contact us for updates." />
            )}
          </div>
        </div>

        <div>
          <Card className="sticky top-20">
            <CardHeader>
              <CardTitle>Request Information</CardTitle>
            </CardHeader>
            <CardContent>
              <InquiryForm projectId={project.projectId} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
