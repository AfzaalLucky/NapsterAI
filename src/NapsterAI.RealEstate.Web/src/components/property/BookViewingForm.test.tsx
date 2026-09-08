import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { api } from "@/api/client";
import { renderWithProviders, screen, waitFor } from "@/test/test-utils";
import { BookViewingForm } from "./BookViewingForm";

vi.mock("@/api/client", async () => {
  const actual = await vi.importActual<typeof import("@/api/client")>("@/api/client");
  return {
    ...actual,
    api: {
      ...actual.api,
      viewings: { ...actual.api.viewings, create: vi.fn() },
    },
  };
});

describe("BookViewingForm", () => {
  beforeEach(() => {
    vi.mocked(api.viewings.create).mockReset();
  });

  it("blocks submission and shows errors when required fields are empty", async () => {
    const user = userEvent.setup();
    renderWithProviders(<BookViewingForm inventoryId={5} />);

    await user.click(screen.getByRole("button", { name: /book viewing/i }));

    expect(await screen.findByText("Name is required")).toBeInTheDocument();
    expect(screen.getByText("Enter a valid email address")).toBeInTheDocument();
    expect(screen.getByText("Pick a date and time")).toBeInTheDocument();
    expect(api.viewings.create).not.toHaveBeenCalled();
  });

  it("submits with valid values, converting the local datetime to ISO and attaching inventoryId", async () => {
    vi.mocked(api.viewings.create).mockResolvedValue({
      id: 1,
      inventoryId: 5,
      status: "Scheduled",
    } as never);
    const onSuccess = vi.fn();
    const user = userEvent.setup();
    renderWithProviders(<BookViewingForm inventoryId={5} onSuccess={onSuccess} />);

    await user.type(screen.getByLabelText(/name/i), "Jane Doe");
    await user.type(screen.getByLabelText(/email/i), "jane@example.com");
    const dateInput = screen.getByLabelText(/preferred date/i);
    await user.type(dateInput, "2030-01-15T10:30");
    await user.click(screen.getByRole("button", { name: /book viewing/i }));

    await waitFor(() => {
      expect(api.viewings.create).toHaveBeenCalledWith(
        expect.objectContaining({
          customerName: "Jane Doe",
          customerEmail: "jane@example.com",
          inventoryId: 5,
          scheduledDate: new Date("2030-01-15T10:30").toISOString(),
        }),
      );
    });
    await waitFor(() => expect(onSuccess).toHaveBeenCalled());
  });
});
