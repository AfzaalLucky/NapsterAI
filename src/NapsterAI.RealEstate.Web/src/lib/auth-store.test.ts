import { beforeEach, describe, expect, it } from "vitest";
import type { AuthResult } from "@/api/types";
import { hasRole, isAuthenticated, useAuthStore } from "./auth-store";

const authResult: AuthResult = {
  accessToken: "access-123",
  accessTokenExpiresAt: "2030-01-01T00:00:00Z",
  refreshToken: "refresh-123",
  refreshTokenExpiresAt: "2030-02-01T00:00:00Z",
  email: "agent@example.com",
  role: "Agent",
};

describe("useAuthStore", () => {
  beforeEach(() => {
    useAuthStore.setState({ accessToken: null, refreshToken: null, email: null, role: null });
  });

  it("starts logged out", () => {
    expect(isAuthenticated()).toBe(false);
    expect(useAuthStore.getState().accessToken).toBeNull();
  });

  it("setSession stores tokens, email and role from the auth result", () => {
    useAuthStore.getState().setSession(authResult);

    const state = useAuthStore.getState();
    expect(state.accessToken).toBe("access-123");
    expect(state.refreshToken).toBe("refresh-123");
    expect(state.email).toBe("agent@example.com");
    expect(state.role).toBe("Agent");
    expect(isAuthenticated()).toBe(true);
  });

  it("logout clears the session", () => {
    useAuthStore.getState().setSession(authResult);
    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.accessToken).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.email).toBeNull();
    expect(state.role).toBeNull();
    expect(isAuthenticated()).toBe(false);
  });

  it("hasRole checks the current role against the given list", () => {
    useAuthStore.getState().setSession(authResult);

    expect(hasRole("Agent")).toBe(true);
    expect(hasRole("Admin")).toBe(false);
    expect(hasRole("Admin", "Agent")).toBe(true);
  });

  it("hasRole is false when logged out", () => {
    expect(hasRole("Admin", "Agent")).toBe(false);
  });
});
