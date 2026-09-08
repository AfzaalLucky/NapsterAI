import { Outlet } from "react-router-dom";
import { RealEstateChatWidget } from "@/components/chat/RealEstateChatWidget";
import { PublicFooter } from "./PublicFooter";
import { PublicHeader } from "./PublicHeader";

export function PublicLayout() {
  return (
    <div className="flex min-h-svh flex-col">
      <PublicHeader />
      <main className="flex-1">
        <Outlet />
      </main>
      <PublicFooter />
      <RealEstateChatWidget />
    </div>
  );
}
