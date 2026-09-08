import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AdminLayout } from "@/components/layout/AdminLayout";
import { Toaster } from "@/components/ui/sonner";
import { PublicLayout } from "@/components/layout/PublicLayout";
import { RequireAuth } from "@/components/layout/RequireAuth";
import { AgentsAdminPage } from "@/routes/admin/AgentsAdminPage";
import { DashboardPage } from "@/routes/admin/DashboardPage";
import { InventoryAdminPage } from "@/routes/admin/InventoryAdminPage";
import { LeadDetailPage } from "@/routes/admin/LeadDetailPage";
import { LeadsAdminPage } from "@/routes/admin/LeadsAdminPage";
import { OrganizationsAdminPage } from "@/routes/admin/OrganizationsAdminPage";
import { ProjectFormPage } from "@/routes/admin/ProjectFormPage";
import { ProjectsAdminPage } from "@/routes/admin/ProjectsAdminPage";
import { ViewingsAdminPage } from "@/routes/admin/ViewingsAdminPage";
import { LoginPage } from "@/routes/auth/LoginPage";
import { ContactPage } from "@/routes/public/ContactPage";
import { HomePage } from "@/routes/public/HomePage";
import { LandingPage } from "@/routes/public/LandingPage";
import { ProjectDetailPage } from "@/routes/public/ProjectDetailPage";
import { SearchPage } from "@/routes/public/SearchPage";
import { UnitDetailPage } from "@/routes/public/UnitDetailPage";
import { WebSdkDemoPage } from "@/routes/webmcp-demo/WebSdkDemoPage";
import { registerRealEstateTools } from "@/lib/edge-mcp/registerRealEstateTools";

// Called once for the app's lifetime - registers the 7 EdgeMCP tools on document.modelContext
// (installing the WebMCP polyfill first if needed) so the chat widget's companion can discover
// and call them the moment a session starts. See registerRealEstateTools.ts for the tool list.
registerRealEstateTools();

const router = createBrowserRouter([
  {
    element: <PublicLayout />,
    children: [
      { path: "/", element: <HomePage /> },
      { path: "/search", element: <SearchPage /> },
      { path: "/projects/:projectId", element: <ProjectDetailPage /> },
      { path: "/inventory/:inventoryId", element: <UnitDetailPage /> },
      { path: "/contact", element: <ContactPage /> },
      { path: "/l/:slug", element: <LandingPage /> },
      { path: "/webmcp-demo", element: <WebSdkDemoPage /> },
    ],
  },
  { path: "/login", element: <LoginPage /> },
  {
    path: "/admin",
    element: <RequireAuth />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: "projects", element: <ProjectsAdminPage /> },
          { path: "projects/new", element: <ProjectFormPage /> },
          { path: "projects/:projectId/edit", element: <ProjectFormPage /> },
          { path: "inventory", element: <InventoryAdminPage /> },
          { path: "leads", element: <LeadsAdminPage /> },
          { path: "leads/:leadId", element: <LeadDetailPage /> },
          { path: "viewings", element: <ViewingsAdminPage /> },
          {
            // Agency/agent management is AdminOnly on the backend too - see Program.cs's
            // "AdminOnly" policy on RealEstateOrganizationsController/RealEstateAgentsController writes.
            element: <RequireAuth roles={["Admin"]} />,
            children: [
              { path: "agents", element: <AgentsAdminPage /> },
              { path: "organizations", element: <OrganizationsAdminPage /> },
            ],
          },
        ],
      },
    ],
  },
]);

export default function App() {
  return (
    <>
      <RouterProvider router={router} />
      <Toaster position="top-center" richColors />
    </>
  );
}
