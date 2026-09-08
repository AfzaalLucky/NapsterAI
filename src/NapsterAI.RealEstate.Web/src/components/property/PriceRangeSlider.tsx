import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";
import { formatPrice } from "@/lib/format";

export function PriceRangeSlider({
  min,
  max,
  step = 50_000,
  value,
  currency = "AED",
  onChange,
}: {
  min: number;
  max: number;
  step?: number;
  value: [number, number];
  currency?: string;
  onChange: (value: [number, number]) => void;
}) {
  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <Label>Price range</Label>
        <span className="text-muted-foreground text-xs">
          {formatPrice(value[0], currency)} – {formatPrice(value[1], currency)}
        </span>
      </div>
      <Slider
        min={min}
        max={max}
        step={step}
        value={value}
        onValueChange={(next) => onChange([next[0], next[1]] as [number, number])}
      />
    </div>
  );
}
