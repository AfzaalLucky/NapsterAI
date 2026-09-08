import { beforeEach, describe, expect, it, vi } from "vitest";
import { useAuthStore } from "@/lib/auth-store";
import { api, ApiError } from "./client";

function jsonResponse(body: unknown, init?: ResponseInit): Response {
  return new Response(JSON.stringify(body), {
    status: 200,
    headers: { "Content-Type": "application/json" },
    ...init,
  });
}

describe("api client", () => {
  beforeEach(() => {
    useAuthStore.setState({ accessToken: null, refreshToken: null, email: null, role: null });
    vi.stubGlobal("fetch", vi.fn());
  });

  it("does not send an Authorization header when logged out", async () => {
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ items: [], totalCount: 0 }));

    await api.projects.list({});

    const [, init] = vi.mocked(fetch).mock.calls[0]!;
    expect((init!.headers as Record<string, string>).Authorization).toBeUndefined();
  });

  it("sends a Bearer Authorization header once a session is set", async () => {
    useAuthStore.getState().setSession({
      accessToken: "token-abc",
      accessTokenExpiresAt: "2030-01-01T00:00:00Z",
      refreshToken: "refresh-abc",
      refreshTokenExpiresAt: "2030-02-01T00:00:00Z",
      email: "agent@example.com",
      role: "Agent",
    });
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ items: [], totalCount: 0 }));

    await api.projects.list({});

    const [, init] = vi.mocked(fetch).mock.calls[0]!;
    expect((init!.headers as Record<string, string>).Authorization).toBe("Bearer token-abc");
  });

  it("builds a query string from defined, non-empty params only", async () => {
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ items: [], totalCount: 0 }));

    await api.projects.list({ city: "Dubai", search: "", isFeatured: true, minPrice: undefined });

    const [url] = vi.mocked(fetch).mock.calls[0]!;
    expect(url).toContain("city=Dubai");
    expect(url).toContain("isFeatured=true");
    expect(url).not.toContain("search=");
    expect(url).not.toContain("minPrice");
  });

  it("logs the session out on a 401 response", async () => {
    useAuthStore.getState().setSession({
      accessToken: "token-abc",
      accessTokenExpiresAt: "2030-01-01T00:00:00Z",
      refreshToken: "refresh-abc",
      refreshTokenExpiresAt: "2030-02-01T00:00:00Z",
      email: "agent@example.com",
      role: "Agent",
    });
    vi.mocked(fetch).mockResolvedValue(
      jsonResponse({ title: "Unauthorized" }, { status: 401, statusText: "Unauthorized" }),
    );

    await expect(api.projects.list({})).rejects.toBeInstanceOf(ApiError);

    expect(useAuthStore.getState().accessToken).toBeNull();
  });

  it("throws an ApiError with the problem detail message on a non-ok response", async () => {
    vi.mocked(fetch).mockResolvedValue(
      jsonResponse({ detail: "Project not found" }, { status: 404, statusText: "Not Found" }),
    );

    const error = await api.projects.get(999).catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(404);
    expect((error as ApiError).message).toBe("Project not found");
  });

  it("falls back to statusText when the error body isn't valid JSON", async () => {
    vi.mocked(fetch).mockResolvedValue(
      new Response("not json", { status: 500, statusText: "Internal Server Error" }),
    );

    const error = await api.projects.get(1).catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).message).toBe("Internal Server Error");
  });

  it("returns undefined for a 204 No Content response", async () => {
    vi.mocked(fetch).mockResolvedValue(new Response(null, { status: 204 }));

    await expect(api.projects.delete(1)).resolves.toBeUndefined();
  });
});
