import { Navigate, Outlet, useLocation } from "react-router-dom";
import { type RealEstateRole, useAuthStore } from "@/lib/auth-store";

/**
 * Gates the /admin/* subtree. Backend [Authorize] policies (AgentOrAdmin/AdminOnly - see
 * Program.cs) are the real enforcement; this only avoids flashing admin UI/making doomed API
 * calls for a signed-out visitor, and is a courtesy for role-restricted pages (roles prop).
 */
export function RequireAuth({ roles }: { roles?: RealEstateRole[] }) {
  const { accessToken, role } = useAuthStore();
  const location = useLocation();

  if (!accessToken) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && (role === null || !roles.includes(role))) {
    return <Navigate to="/admin" replace />;
  }

  return <Outlet />;
}
