import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { api } from "@/api/client";
import { renderWithProviders, screen, waitFor } from "@/test/test-utils";
import { InquiryForm } from "./InquiryForm";

vi.mock("@/api/client", async () => {
  const actual = await vi.importActual<typeof import("@/api/client")>("@/api/client");
  return {
    ...actual,
    api: {
      ...actual.api,
      inquiries: { ...actual.api.inquiries, create: vi.fn() },
    },
  };
});

describe("InquiryForm", () => {
  beforeEach(() => {
    vi.mocked(api.inquiries.create).mockReset();
  });

  it("blocks submission and shows errors when required fields are empty", async () => {
    const user = userEvent.setup();
    renderWithProviders(<InquiryForm />);

    await user.click(screen.getByRole("button", { name: /send inquiry/i }));

    expect(await screen.findByText("Name is required")).toBeInTheDocument();
    expect(screen.getByText("Enter a valid email address")).toBeInTheDocument();
    expect(api.inquiries.create).not.toHaveBeenCalled();
  });

  it("rejects an invalid email without calling the API", async () => {
    const user = userEvent.setup();
    renderWithProviders(<InquiryForm />);

    // "user@localhost" satisfies the browser's native type=email constraint (so the submit
    // event actually reaches react-hook-form) but fails zod's stricter TLD-requiring pattern.
    await user.type(screen.getByLabelText(/name/i), "Jane Doe");
    await user.type(screen.getByLabelText(/email/i), "user@localhost");
    await user.click(screen.getByRole("button", { name: /send inquiry/i }));

    expect(await screen.findByText("Enter a valid email address")).toBeInTheDocument();
    expect(api.inquiries.create).not.toHaveBeenCalled();
  });

  it("submits with valid values, forwarding projectId/inventoryId and the Website channel", async () => {
    vi.mocked(api.inquiries.create).mockResolvedValue({
      id: 1,
      customerName: "Jane Doe",
      customerEmail: "jane@example.com",
      channel: "Website",
      status: "New",
      createdAt: new Date().toISOString(),
    } as never);
    const onSuccess = vi.fn();
    const user = userEvent.setup();
    renderWithProviders(<InquiryForm projectId={7} inventoryId={42} onSuccess={onSuccess} />);

    await user.type(screen.getByLabelText(/name/i), "Jane Doe");
    await user.type(screen.getByLabelText(/email/i), "jane@example.com");
    await user.click(screen.getByRole("button", { name: /send inquiry/i }));

    await waitFor(() => {
      expect(api.inquiries.create).toHaveBeenCalledWith({
        customerName: "Jane Doe",
        customerEmail: "jane@example.com",
        customerPhone: "",
        message: "",
        projectId: 7,
        inventoryId: 42,
        channel: "Website",
      });
    });
    await waitFor(() => expect(onSuccess).toHaveBeenCalled());
  });
});
