import { SearchIcon, SparklesIcon } from "lucide-react";
import { type FormEvent, useState } from "react";
import { Helmet } from "react-helmet-async";
import { Link, useNavigate } from "react-router-dom";
import { PropertyCard } from "@/components/property/PropertyCard";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useProjectsQuery } from "@/features/projects/useProjects";

export function HomePage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");

  const { data: featured, isPending } = useProjectsQuery({ isFeatured: true, pageSize: 6 });

  function handleSearch(event: FormEvent) {
    event.preventDefault();
    const params = new URLSearchParams();
    if (search.trim()) {
      params.set("search", search.trim());
    }
    navigate(`/search${params.toString() ? `?${params}` : ""}`);
  }

  return (
    <div>
      <Helmet>
        <title>NapsterAI Real Estate</title>
        <meta
          name="description"
          content="Search featured developments, browse available units, and talk to our AI assistant to book a viewing."
        />
      </Helmet>

      <section className="mx-auto max-w-7xl px-4 py-16 text-center sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">Find your next property</h1>
        <p className="mx-auto mt-4 max-w-2xl text-muted-foreground">
          Search featured developments, browse available units, and talk to our AI assistant to book a viewing.
        </p>

        <form onSubmit={handleSearch} className="mx-auto mt-8 flex max-w-xl gap-2">
          <Input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by project name, city, or code…"
            aria-label="Search projects"
          />
          <Button type="submit">
            <SearchIcon />
            Search
          </Button>
        </form>

        <div className="mt-4 flex justify-center gap-3">
          <Button asChild variant="outline" size="sm">
            <Link to="/contact">Contact Us</Link>
          </Button>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 pb-16 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold">Featured Developments</h2>
          <Link to="/search?featured=true" className="text-primary text-sm font-medium hover:underline">
            View all
          </Link>
        </div>

        <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {isPending
            ? Array.from({ length: 3 }, (_, i) => <Skeleton key={i} className="aspect-[4/5] w-full" />)
            : featured?.items.map((project) => <PropertyCard key={project.projectId} project={project} />)}
        </div>
      </section>

      <section className="border-t bg-accent/40">
        <div className="mx-auto flex max-w-7xl flex-col items-center gap-4 px-4 py-16 text-center sm:px-6 lg:px-8">
          <SparklesIcon className="text-primary size-8" />
          <h2 className="text-xl font-semibold">Not sure where to start?</h2>
          <p className="text-muted-foreground max-w-xl">
            Chat with our AI assistant - describe what you're looking for and it will search listings, calculate
            payment plans, and book viewings for you.
          </p>
          <Button asChild>
            <Link to="/webmcp-demo">Try the AI Assistant</Link>
          </Button>
        </div>
      </section>
    </div>
  );
}
