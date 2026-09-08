import { useNavigate } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useAuthStore } from "@/lib/auth-store";

export function AdminTopbar() {
  const { email, role, logout } = useAuthStore();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <header className="flex h-16 items-center justify-between border-b px-4 sm:px-6">
      <span className="text-sm font-semibold">Real Estate Admin</span>

      <div className="flex items-center gap-3">
        {role && <Badge variant="secondary">{role}</Badge>}
        <span className="hidden text-sm text-muted-foreground sm:inline">{email}</span>
        <Button variant="outline" size="sm" onClick={handleLogout}>
          Log out
        </Button>
      </div>
    </header>
  );
}
