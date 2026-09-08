import { NavLink } from "react-router-dom";
import { useAuthStore } from "@/lib/auth-store";
import { cn } from "@/lib/utils";

const navItems = [
  { to: "/admin", label: "Dashboard", end: true, adminOnly: false },
  { to: "/admin/projects", label: "Projects", adminOnly: false },
  { to: "/admin/inventory", label: "Inventory", adminOnly: false },
  { to: "/admin/leads", label: "Leads", adminOnly: false },
  { to: "/admin/viewings", label: "Viewings", adminOnly: false },
  { to: "/admin/agents", label: "Sales Agents", adminOnly: true },
  { to: "/admin/organizations", label: "Organizations", adminOnly: true },
];

const linkClassName = ({ isActive }: { isActive: boolean }) =>
  cn(
    "rounded-md px-3 py-2 text-sm font-medium text-muted-foreground outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:ring-[3px] focus-visible:ring-ring/50",
    isActive && "bg-accent text-accent-foreground",
  );

function useVisibleNavItems() {
  const role = useAuthStore((state) => state.role);
  return navItems.filter((item) => !item.adminOnly || role === "Admin");
}

export function AdminSidebar() {
  const items = useVisibleNavItems();

  return (
    <aside className="hidden w-56 shrink-0 border-r md:block">
      <nav className="flex flex-col gap-1 p-4">
        {items.map((item) => (
          <NavLink key={item.to} to={item.to} end={item.end} className={linkClassName}>
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}

// Below md, AdminSidebar's <aside> is hidden (no room) - this horizontal, scrollable bar,
// rendered as its own row above the sidebar+main flex layout (not inside it), is the only way
// to reach non-Dashboard admin sections on a phone, so it isn't a dead end.
export function AdminMobileNav() {
  const items = useVisibleNavItems();

  return (
    <nav className="flex gap-1 overflow-x-auto border-b p-2 md:hidden">
      {items.map((item) => (
        <NavLink key={item.to} to={item.to} end={item.end} className={linkClassName}>
          <span className="whitespace-nowrap">{item.label}</span>
        </NavLink>
      ))}
    </nav>
  );
}
