import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthResult } from "@/api/types";

export type RealEstateRole = "Admin" | "Agent";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  email: string | null;
  role: RealEstateRole | null;
  setSession: (result: AuthResult) => void;
  logout: () => void;
}

/**
 * Persisted to localStorage so a refresh doesn't bounce the user back to /login. Only auth
 * state lives in global client state (per RealEstateImplementationPlan.md's decision to skip
 * Redux Toolkit here) - everything else is server state, owned by TanStack Query.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      email: null,
      role: null,
      setSession: (result) =>
        set({
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
          email: result.email,
          role: result.role as RealEstateRole,
        }),
      logout: () => set({ accessToken: null, refreshToken: null, email: null, role: null }),
    }),
    { name: "realestate-auth" },
  ),
);

export function isAuthenticated(): boolean {
  return useAuthStore.getState().accessToken !== null;
}

export function hasRole(...roles: RealEstateRole[]): boolean {
  const role = useAuthStore.getState().role;
  return role !== null && roles.includes(role);
}
