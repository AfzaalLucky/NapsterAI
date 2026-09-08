import { useQuery } from "@tanstack/react-query";
import { Helmet } from "react-helmet-async";
import { Link, useParams } from "react-router-dom";
import { api } from "@/api/client";
import { EmptyState } from "@/components/EmptyState";
import { InquiryForm } from "@/components/property/InquiryForm";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { formatPrice } from "@/lib/format";

/**
 * Campaign/ad landing page for a single project, addressed by its ProjectCode as the "slug"
 * (e.g. /l/PRJ-001) - there's no separate CMS/slug table, so this reuses the catalog search
 * endpoint (which already matches ProjectName or ProjectCode) rather than adding one.
 */
export function LandingPage() {
  const { slug } = useParams<{ slug: string }>();

  const { data, isPending } = useQuery({
    queryKey: ["projects", "by-code", slug],
    queryFn: () => api.projects.list({ search: slug, pageSize: 5 }),
    enabled: Boolean(slug),
  });

  const project = data?.items.find((p) => p.projectCode.toLowerCase() === slug?.toLowerCase()) ?? data?.items[0];

  if (isPending) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!project) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
        <EmptyState title="Campaign not found" description={`No project matches "${slug}".`} />
      </div>
    );
  }

  return (
    <div>
      <Helmet>
        <title>{project.projectName} | NapsterAI Real Estate</title>
      </Helmet>

      <section
        className="relative flex min-h-[60vh] flex-col items-center justify-center gap-4 bg-cover bg-center px-4 text-center text-white"
        style={{
          backgroundImage: project.imageUrl
            ? `linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url(${project.imageUrl})`
            : undefined,
          backgroundColor: project.imageUrl ? undefined : "var(--color-foreground)",
        }}
      >
        <p className="text-sm tracking-widest uppercase opacity-80">{project.developer}</p>
        <h1 className="text-4xl font-bold sm:text-5xl">{project.projectName}</h1>
        <p className="max-w-xl opacity-90">
          {project.district ? `${project.district}, ` : ""}
          {project.city}, {project.country}
        </p>
        {project.startingPrice != null && (
          <p className="text-lg font-semibold">Starting from {formatPrice(project.startingPrice, project.currency)}</p>
        )}
        <Button asChild size="lg" variant="secondary">
          <Link to={`/projects/${project.projectId}`}>Explore this development</Link>
        </Button>
      </section>

      <section className="mx-auto max-w-3xl px-4 py-12 sm:px-6 lg:px-8">
        <Card>
          <CardHeader>
            <CardTitle>Get more details</CardTitle>
          </CardHeader>
          <CardContent>
            <InquiryForm projectId={project.projectId} />
          </CardContent>
        </Card>
      </section>
    </div>
  );
}
