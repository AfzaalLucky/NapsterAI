import { PriceRangeSlider } from "@/components/property/PriceRangeSlider";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useLocationsQuery, useLookupsQuery } from "@/features/projects/useReferenceData";

export const PRICE_FILTER_MIN = 0;
export const PRICE_FILTER_MAX = 20_000_000;

export interface PropertyFiltersValue {
  search: string;
  city: string;
  projectType: string;
  isFeatured: boolean;
  priceRange: [number, number];
}

const ANY_VALUE = "any";

export function PropertyFilters({
  value,
  onChange,
  onClear,
}: {
  value: PropertyFiltersValue;
  onChange: (patch: Partial<PropertyFiltersValue>) => void;
  onClear: () => void;
}) {
  const { data: locations } = useLocationsQuery();
  const { data: projectTypes } = useLookupsQuery("ProjectType");

  const cities = [...new Set((locations ?? []).map((l) => l.city))].sort();

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1.5">
        <Label htmlFor="filter-search">Search</Label>
        <Input
          id="filter-search"
          placeholder="Project name or code…"
          value={value.search}
          onChange={(e) => onChange({ search: e.target.value })}
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>City</Label>
        <Select
          value={value.city || ANY_VALUE}
          onValueChange={(next) => onChange({ city: next === ANY_VALUE ? "" : next })}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder="Any city" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ANY_VALUE}>Any city</SelectItem>
            {cities.map((city) => (
              <SelectItem key={city} value={city}>
                {city}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Property type</Label>
        <Select
          value={value.projectType || ANY_VALUE}
          onValueChange={(next) => onChange({ projectType: next === ANY_VALUE ? "" : next })}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder="Any type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ANY_VALUE}>Any type</SelectItem>
            {(projectTypes ?? []).map((lookup) => (
              <SelectItem key={lookup.lookupId} value={lookup.code}>
                {lookup.displayName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <PriceRangeSlider
        min={PRICE_FILTER_MIN}
        max={PRICE_FILTER_MAX}
        step={100_000}
        value={value.priceRange}
        onChange={(priceRange) => onChange({ priceRange })}
      />

      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          className="accent-primary size-4"
          checked={value.isFeatured}
          onChange={(e) => onChange({ isFeatured: e.target.checked })}
        />
        Featured only
      </label>

      <Button variant="outline" onClick={onClear}>
        Clear filters
      </Button>
    </div>
  );
}
