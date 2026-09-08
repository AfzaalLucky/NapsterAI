import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { ApiError } from "@/api/client";
import { ProjectStatuses, ProjectTypes } from "@/api/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useCreateProject, useUpdateProject } from "@/features/projects/useProjectMutations";
import { useProjectQuery } from "@/features/projects/useProjects";

const projectSchema = z.object({
  projectCode: z.string().min(1, "Project code is required"),
  projectName: z.string().min(1, "Project name is required"),
  developer: z.string().optional(),
  projectType: z.enum(ProjectTypes, { message: "Select a project type" }),
  status: z.enum(ProjectStatuses, { message: "Select a status" }),
  country: z.string().min(1, "Country is required"),
  city: z.string().min(1, "City is required"),
  district: z.string().optional(),
  currency: z.string().min(1, "Currency is required"),
  startingPrice: z.number().nonnegative().optional(),
  maxPrice: z.number().nonnegative().optional(),
  imageUrl: z.union([z.url(), z.literal("")]).optional(),
  description: z.string().optional(),
  contactPerson: z.string().optional(),
  contactPhone: z.string().optional(),
  contactEmail: z.union([z.email(), z.literal("")]).optional(),
  isFeatured: z.boolean(),
  isActive: z.boolean(),
});

type ProjectFormValues = z.infer<typeof projectSchema>;

const DEFAULT_VALUES: ProjectFormValues = {
  projectCode: "",
  projectName: "",
  developer: "",
  projectType: "Residential",
  status: "Planning",
  country: "",
  city: "",
  district: "",
  currency: "AED",
  imageUrl: "",
  description: "",
  contactPerson: "",
  contactPhone: "",
  contactEmail: "",
  isFeatured: false,
  isActive: true,
};

export function ProjectFormPage() {
  const { projectId: projectIdParam } = useParams<{ projectId: string }>();
  const isEditing = Boolean(projectIdParam);
  const projectId = Number(projectIdParam);
  const navigate = useNavigate();

  const { data: existing } = useProjectQuery(projectId);
  const createProject = useCreateProject();
  const updateProject = useUpdateProject(projectId);

  const form = useForm<ProjectFormValues>({
    resolver: zodResolver(projectSchema),
    defaultValues: DEFAULT_VALUES,
  });

  useEffect(() => {
    if (existing) {
      form.reset({
        projectCode: existing.projectCode,
        projectName: existing.projectName,
        developer: existing.developer ?? "",
        projectType: existing.projectType as ProjectFormValues["projectType"],
        status: existing.status as ProjectFormValues["status"],
        country: existing.country,
        city: existing.city,
        district: existing.district ?? "",
        currency: existing.currency,
        startingPrice: existing.startingPrice ?? undefined,
        maxPrice: existing.maxPrice ?? undefined,
        imageUrl: existing.imageUrl ?? "",
        description: existing.description ?? "",
        contactPerson: existing.contactPerson ?? "",
        contactPhone: existing.contactPhone ?? "",
        contactEmail: existing.contactEmail ?? "",
        isFeatured: existing.isFeatured,
        isActive: existing.isActive,
      });
    }
  }, [existing, form]);

  const mutationHandlers = {
    onSuccess: () => {
      toast.success(isEditing ? "Project updated." : "Project created.");
      navigate("/admin/projects");
    },
    onError: (error: unknown) => {
      toast.error(error instanceof ApiError ? error.message : "Could not save the project.");
    },
  };

  function onSubmit(values: ProjectFormValues) {
    const body = {
      ...values,
      developer: values.developer || undefined,
      district: values.district || undefined,
      imageUrl: values.imageUrl || undefined,
      description: values.description || undefined,
      contactPerson: values.contactPerson || undefined,
      contactPhone: values.contactPhone || undefined,
      contactEmail: values.contactEmail || undefined,
    };

    if (isEditing) {
      updateProject.mutate(body, mutationHandlers);
    } else {
      createProject.mutate(body, mutationHandlers);
    }
  }

  const isSaving = createProject.isPending || updateProject.isPending;

  return (
    <div className="mx-auto max-w-2xl">
      <h1 className="text-2xl font-semibold">{isEditing ? `Edit Project` : "New Project"}</h1>

      <Card className="mt-4">
        <CardHeader>
          <CardTitle>Project Details</CardTitle>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-4">
              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="projectCode"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Project Code</FormLabel>
                      <FormControl>
                        <Input {...field} disabled={isEditing} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="projectName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Project Name</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="developer"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Developer</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="projectType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Type</FormLabel>
                      <Select value={field.value} onValueChange={field.onChange}>
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {ProjectTypes.map((type) => (
                            <SelectItem key={type} value={type}>
                              {type}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="status"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Status</FormLabel>
                      <Select value={field.value} onValueChange={field.onChange}>
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {ProjectStatuses.map((status) => (
                            <SelectItem key={status} value={status}>
                              {status}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <FormField
                  control={form.control}
                  name="country"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Country</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="city"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>City</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="district"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>District</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <FormField
                  control={form.control}
                  name="currency"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Currency</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="startingPrice"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Starting Price</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value === "" ? undefined : Number(e.target.value))}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="maxPrice"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Max Price</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value === "" ? undefined : Number(e.target.value))}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="imageUrl"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Hero Image URL</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Textarea rows={4} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-3 gap-4">
                <FormField
                  control={form.control}
                  name="contactPerson"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Contact Person</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="contactPhone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Contact Phone</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="contactEmail"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Contact Email</FormLabel>
                      <FormControl>
                        <Input type="email" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="flex items-center gap-6">
                <FormField
                  control={form.control}
                  name="isFeatured"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center gap-2">
                      <FormControl>
                        <Switch checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                      <FormLabel className="!mt-0">Featured</FormLabel>
                    </FormItem>
                  )}
                />
                {isEditing && (
                  <FormField
                    control={form.control}
                    name="isActive"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center gap-2">
                        <FormControl>
                          <Switch checked={field.value} onCheckedChange={field.onChange} />
                        </FormControl>
                        <FormLabel className="!mt-0">Active</FormLabel>
                      </FormItem>
                    )}
                  />
                )}
              </div>

              <div className="mt-2 flex gap-2">
                <Button type="submit" disabled={isSaving}>
                  {isSaving ? "Saving…" : isEditing ? "Save changes" : "Create project"}
                </Button>
                <Button type="button" variant="outline" onClick={() => navigate("/admin/projects")}>
                  Cancel
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}
