import { ChevronRightIcon } from "lucide-react";
import { Helmet } from "react-helmet-async";
import { Link, useParams } from "react-router-dom";
import { EmptyState } from "@/components/EmptyState";
import { BookViewingForm } from "@/components/property/BookViewingForm";
import { InquiryForm } from "@/components/property/InquiryForm";
import { PaymentPlanCalculator } from "@/components/property/PaymentPlanCalculator";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useInventoryQuery } from "@/features/inventory/useInventory";
import { useProjectQuery } from "@/features/projects/useProjects";
import { formatPrice } from "@/lib/format";

export function UnitDetailPage() {
  const { inventoryId: inventoryIdParam } = useParams<{ inventoryId: string }>();
  const inventoryId = Number(inventoryIdParam);

  const { data: unit, isPending } = useInventoryQuery(inventoryId);
  const { data: project } = useProjectQuery(unit?.projectId ?? Number.NaN);

  if (isPending) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="mt-4 h-64 w-full" />
      </div>
    );
  }

  if (!unit) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <EmptyState title="Unit not found" description="This listing may have been removed or sold." />
      </div>
    );
  }

  const currency = project?.currency ?? "AED";
  const specs: [string, string][] = [
    ["Bedrooms", unit.bedrooms?.toString() ?? "—"],
    ["Bathrooms", unit.bathrooms?.toString() ?? "—"],
    ["Area", unit.areaSqFt ? `${unit.areaSqFt} sq ft` : "—"],
    ["Floor", unit.floorNumber?.toString() ?? "—"],
    ["View", unit.viewType ?? "—"],
    ["Furnishing", unit.furnishingStatus ?? "—"],
    ["Parking", unit.parkingSpaces?.toString() ?? "—"],
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <Helmet>
        <title>
          Unit {unit.unitNumber}
          {project ? ` - ${project.projectName}` : ""} | NapsterAI Real Estate
        </title>
      </Helmet>

      <nav className="text-muted-foreground mb-4 flex items-center gap-1 text-sm">
        <Link to="/search" className="hover:text-foreground">
          Search
        </Link>
        <ChevronRightIcon className="size-3.5" />
        {project && (
          <>
            <Link to={`/projects/${project.projectId}`} className="hover:text-foreground">
              {project.projectName}
            </Link>
            <ChevronRightIcon className="size-3.5" />
          </>
        )}
        <span className="text-foreground">Unit {unit.unitNumber}</span>
      </nav>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-[1fr_20rem]">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="text-2xl font-semibold">Unit {unit.unitNumber}</h1>
            <Badge variant="outline">{unit.status}</Badge>
          </div>
          {project && <p className="text-muted-foreground mt-1">{project.projectName}</p>}

          <p className="mt-4 text-2xl font-semibold">
            {unit.listPrice != null ? formatPrice(unit.listPrice, currency) : "Price on request"}
          </p>

          <div className="mt-6 grid grid-cols-2 gap-4 sm:grid-cols-3">
            {specs.map(([label, val]) => (
              <div key={label} className="rounded-md border p-3">
                <p className="text-muted-foreground text-xs">{label}</p>
                <p className="font-medium">{val}</p>
              </div>
            ))}
          </div>

          {unit.notes && <p className="mt-6 text-sm leading-relaxed">{unit.notes}</p>}

          {unit.listPrice != null && (
            <div className="mt-8">
              <h2 className="text-lg font-semibold">Payment Plan Calculator</h2>
              <div className="mt-3">
                <PaymentPlanCalculator inventoryId={unit.inventoryId} />
              </div>
            </div>
          )}
        </div>

        <div>
          <Card className="sticky top-20">
            <CardHeader>
              <CardTitle>Interested in this unit?</CardTitle>
            </CardHeader>
            <CardContent>
              <Tabs defaultValue="inquiry">
                <TabsList className="w-full">
                  <TabsTrigger value="inquiry">Request Info</TabsTrigger>
                  <TabsTrigger value="viewing">Book Viewing</TabsTrigger>
                </TabsList>
                <TabsContent value="inquiry" className="pt-4">
                  <InquiryForm projectId={unit.projectId} inventoryId={unit.inventoryId} />
                </TabsContent>
                <TabsContent value="viewing" className="pt-4">
                  <BookViewingForm inventoryId={unit.inventoryId} />
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
