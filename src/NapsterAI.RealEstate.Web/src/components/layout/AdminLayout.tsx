import { Outlet } from "react-router-dom";
import { AdminMobileNav, AdminSidebar } from "./AdminSidebar";
import { AdminTopbar } from "./AdminTopbar";

export function AdminLayout() {
  return (
    <div className="flex min-h-svh flex-col">
      <AdminTopbar />
      <AdminMobileNav />
      <div className="flex flex-1">
        <AdminSidebar />
        <main className="flex-1 p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
