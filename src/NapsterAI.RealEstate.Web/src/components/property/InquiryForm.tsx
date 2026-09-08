import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "@/api/client";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCreateInquiry } from "@/features/inquiries/useCreateInquiry";

const inquirySchema = z.object({
  customerName: z.string().min(1, "Name is required"),
  customerEmail: z.email("Enter a valid email address"),
  customerPhone: z.string().optional(),
  message: z.string().optional(),
});

type InquiryFormValues = z.infer<typeof inquirySchema>;

/** Backs the public "Request info" form (POST /inquiries) - anonymous, channel="Website". */
export function InquiryForm({
  projectId,
  inventoryId,
  onSuccess,
}: {
  projectId?: number;
  inventoryId?: number;
  onSuccess?: () => void;
}) {
  const form = useForm<InquiryFormValues>({
    resolver: zodResolver(inquirySchema),
    defaultValues: { customerName: "", customerEmail: "", customerPhone: "", message: "" },
  });
  const createInquiry = useCreateInquiry();

  function onSubmit(values: InquiryFormValues) {
    createInquiry.mutate(
      { ...values, projectId, inventoryId, channel: "Website" },
      {
        onSuccess: () => {
          toast.success("Thanks! An agent will be in touch shortly.");
          form.reset();
          onSuccess?.();
        },
        onError: (error) => {
          toast.error(error instanceof ApiError ? error.message : "Could not send your request. Please try again.");
        },
      },
    );
  }

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
          name="message"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Message (optional)</FormLabel>
              <FormControl>
                <Textarea rows={3} {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={createInquiry.isPending}>
          {createInquiry.isPending ? "Sending…" : "Send inquiry"}
        </Button>
      </form>
    </Form>
  );
}
