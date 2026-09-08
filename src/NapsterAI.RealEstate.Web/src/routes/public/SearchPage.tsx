import { SlidersHorizontalIcon } from "lucide-react";
import { useMemo } from "react";
import { Helmet } from "react-helmet-async";
import { useSearchParams } from "react-router-dom";
import {
  PRICE_FILTER_MAX,
  PRICE_FILTER_MIN,
  PropertyFilters,
  type PropertyFiltersValue,
} from "@/components/property/PropertyFilters";
import { PropertyGrid } from "@/components/property/PropertyGrid";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { useProjectsQuery } from "@/features/projects/useProjects";

const PAGE_SIZE = 12;

export function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters: PropertyFiltersValue = useMemo(
    () => ({
      search: searchParams.get("search") ?? "",
      city: searchParams.get("city") ?? "",
      projectType: searchParams.get("projectType") ?? "",
      isFeatured: searchParams.get("featured") === "true",
      priceRange: [
        Number(searchParams.get("minPrice") ?? PRICE_FILTER_MIN),
        Number(searchParams.get("maxPrice") ?? PRICE_FILTER_MAX),
      ],
    }),
    [searchParams],
  );
  const pageIndex = Number(searchParams.get("page") ?? 0);

  function updateFilters(patch: Partial<PropertyFiltersValue>) {
    const next = { ...filters, ...patch };
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev);
      params.delete("page"); // any filter change restarts pagination

      setOrDelete(params, "search", next.search);
      setOrDelete(params, "city", next.city);
      setOrDelete(params, "projectType", next.projectType);
      setOrDelete(params, "featured", next.isFeatured ? "true" : "");
      setOrDelete(params, "minPrice", next.priceRange[0] > PRICE_FILTER_MIN ? String(next.priceRange[0]) : "");
      setOrDelete(params, "maxPrice", next.priceRange[1] < PRICE_FILTER_MAX ? String(next.priceRange[1]) : "");
      return params;
    });
  }

  function clearFilters() {
    setSearchParams(new URLSearchParams());
  }

  function goToPage(nextPage: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev);
      setOrDelete(params, "page", nextPage > 0 ? String(nextPage) : "");
      return params;
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  const { data, isPending, isError } = useProjectsQuery({
    search: filters.search || undefined,
    city: filters.city || undefined,
    projectType: filters.projectType || undefined,
    isFeatured: filters.isFeatured || undefined,
    minPrice: filters.priceRange[0] > PRICE_FILTER_MIN ? filters.priceRange[0] : undefined,
    maxPrice: filters.priceRange[1] < PRICE_FILTER_MAX ? filters.priceRange[1] : undefined,
    pageIndex,
    pageSize: PAGE_SIZE,
  });

  const totalPages = data ? Math.max(1, Math.ceil(data.filteredCount / PAGE_SIZE)) : 1;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <Helmet>
        <title>Search Projects | NapsterAI Real Estate</title>
        <meta
          name="description"
          content="Browse residential, commercial, and mixed-use developments filtered by city, type, and price."
        />
      </Helmet>

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Search Projects</h1>

        <Sheet>
          <SheetTrigger asChild>
            <Button variant="outline" size="sm" className="lg:hidden">
              <SlidersHorizontalIcon />
              Filters
            </Button>
          </SheetTrigger>
          <SheetContent side="left">
            <SheetHeader>
              <SheetTitle>Filters</SheetTitle>
            </SheetHeader>
            <div className="px-4 pb-4">
              <PropertyFilters value={filters} onChange={updateFilters} onClear={clearFilters} />
            </div>
          </SheetContent>
        </Sheet>
      </div>

      <div className="mt-6 grid grid-cols-1 gap-8 lg:grid-cols-[16rem_1fr]">
        <aside className="hidden lg:block">
          <PropertyFilters value={filters} onChange={updateFilters} onClear={clearFilters} />
        </aside>

        <div>
          {isError ? (
            <p className="text-destructive">Could not load projects. Is the API running?</p>
          ) : (
            <>
              <p className="text-muted-foreground mb-4 text-sm">
                {isPending ? "Loading…" : `${data?.filteredCount ?? 0} project(s) found`}
              </p>
              <PropertyGrid projects={data?.items} isLoading={isPending} />

              {!isPending && totalPages > 1 && (
                <Pagination className="mt-8">
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious
                        onClick={() => goToPage(Math.max(0, pageIndex - 1))}
                        aria-disabled={pageIndex === 0}
                        className={pageIndex === 0 ? "pointer-events-none opacity-50" : "cursor-pointer"}
                      />
                    </PaginationItem>
                    {Array.from({ length: totalPages }, (_, i) => (
                      <PaginationItem key={i}>
                        <PaginationLink isActive={i === pageIndex} onClick={() => goToPage(i)}>
                          {i + 1}
                        </PaginationLink>
                      </PaginationItem>
                    ))}
                    <PaginationItem>
                      <PaginationNext
                        onClick={() => goToPage(Math.min(totalPages - 1, pageIndex + 1))}
                        aria-disabled={pageIndex >= totalPages - 1}
                        className={pageIndex >= totalPages - 1 ? "pointer-events-none opacity-50" : "cursor-pointer"}
                      />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

function setOrDelete(params: URLSearchParams, key: string, value: string) {
  if (value) {
    params.set(key, value);
  } else {
    params.delete(key);
  }
}
