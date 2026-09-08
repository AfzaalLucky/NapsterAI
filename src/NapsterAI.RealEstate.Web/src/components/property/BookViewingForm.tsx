import { zodResolver } from "@hookform/resolvers/zod";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "@/api/client";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCreateViewing } from "@/features/viewings/useCreateViewing";

const viewingSchema = z.object({
  customerName: z.string().min(1, "Name is required"),
  customerEmail: z.email("Enter a valid email address"),
  customerPhone: z.string().optional(),
  scheduledDate: z.string().min(1, "Pick a date and time"),
  notes: z.string().optional(),
});

type ViewingFormValues = z.infer<typeof viewingSchema>;

/** Backs the public "Book a viewing" form (POST /viewings) - anonymous, transactionally upserts Customer+Lead+Viewing. */
export function BookViewingForm({ inventoryId, onSuccess }: { inventoryId: number; onSuccess?: () => void }) {
  const form = useForm<ViewingFormValues>({
    resolver: zodResolver(viewingSchema),
    defaultValues: { customerName: "", customerEmail: "", customerPhone: "", scheduledDate: "", notes: "" },
  });
  const createViewing = useCreateViewing();

  function onSubmit(values: ViewingFormValues) {
    createViewing.mutate(
      { ...values, inventoryId, scheduledDate: new Date(values.scheduledDate).toISOString() },
      {
        onSuccess: () => {
          toast.success("Viewing requested! We'll confirm the time with you shortly.");
          form.reset();
          onSuccess?.();
        },
        onError: (error) => {
          toast.error(
            error instanceof ApiError ? error.message : "Could not book the viewing. Please try again.",
          );
        },
      },
    );
  }

  // Computed once per mount rather than on every render - it only needs to be "roughly now".
  const minDate = useMemo(() => new Date(Date.now() + 60 * 60 * 1000).toISOString().slice(0, 16), []);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <FormField
          control={form.control}
          name="customerName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="customerEmail"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="customerPhone"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Phone (optional)</FormLabel>
              <FormControl>
                <Input type="tel" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="scheduledDate"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Preferred date &amp; time</FormLabel>
              <FormControl>
                <Input type="datetime-local" min={minDate} {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Notes (optional)</FormLabel>
              <FormControl>
                <Textarea rows={2} {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={createViewing.isPending}>
          {createViewing.isPending ? "Booking…" : "Book viewing"}
        </Button>
      </form>
    </Form>
  );
}
