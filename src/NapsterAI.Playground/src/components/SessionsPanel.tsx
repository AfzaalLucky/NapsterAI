import { useEffect, useState } from "react";
import { api, ApiError } from "../api/client";
import type { Companion, Session } from "../api/types";

interface Props {
  companion: Companion | null;
  /** Bump this from the parent (e.g. after a live chat session starts/ends) to force a refetch. */
  refreshKey?: number;
}

function formatEpoch(value?: number): string {
  if (!value) return "—";
  // Napster doesn't document seconds vs. milliseconds; treat values below 10^12 as seconds.
  const ms = value < 1_000_000_000_000 ? value * 1000 : value;
  return new Date(ms).toLocaleString();
}

function statusVariant(status?: string): string {
  switch (status) {
    case "active":
    case "open":
      return "live";
    case "closed":
      return "closed";
    case "error":
    case "failed":
      return "error";
    default:
      return "neutral";
  }
}

export function SessionsPanel({ companion, refreshKey }: Props) {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [onlyThisCompanion, setOnlyThisCompanion] = useState(true);
  const [manualRefresh, setManualRefresh] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    api
      .listSessions({
        companionId: onlyThisCompanion && companion ? companion.id : undefined,
        pageSize: 20,
      })
      .then((result) => {
        if (!cancelled) setSessions(result.items);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Failed to load sessions.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [companion, onlyThisCompanion, refreshKey, manualRefresh]);

  return (
    <div className="panel">
      <div className="panel-header">
        <h2>Sessions</h2>
        <button type="button" className="link-button" onClick={() => setManualRefresh((n) => n + 1)}>
          Refresh
        </button>
      </div>

      <label className="checkbox-list__item">
        <input
          type="checkbox"
          checked={onlyThisCompanion}
          onChange={(e) => setOnlyThisCompanion(e.target.checked)}
          disabled={!companion}
        />
        Only show sessions for the selected companion
      </label>

      {error && <p className="error">{error}</p>}

      <table className="data-table">
        <thead>
          <tr>
            <th>Companion</th>
            <th>Agent</th>
            <th>Type</th>
            <th>Status</th>
            <th>Cost</th>
            <th>Started</th>
            <th>Closed</th>
          </tr>
        </thead>
        <tbody>
          {loading &&
            sessions.length === 0 &&
            Array.from({ length: 3 }).map((_, i) => (
              <tr key={`skeleton-${i}`} aria-hidden="true">
                <td colSpan={7}>
                  <div className="skeleton skeleton--line" />
                </td>
              </tr>
            ))}

          {!loading &&
            sessions.map((s) => (
              <tr key={s.id}>
                <td>
                  {s.companionFirstName} {s.companionLastName}
                </td>
                <td>{s.agentName ?? "—"}</td>
                <td>
                  {s.sessionType ?? "—"} / {s.modality ?? "—"}
                </td>
                <td>
                  <span className={`badge badge--${statusVariant(s.status)}`}>{s.status ?? "unknown"}</span>
                  {s.closeReason && <span className="hint"> ({s.closeReason})</span>}
                </td>
                <td>{s.cost != null ? `$${s.cost.toFixed(2)}` : "—"}</td>
                <td>{formatEpoch(s.startedAt)}</td>
                <td>{formatEpoch(s.closedAt)}</td>
              </tr>
            ))}

          {!loading && sessions.length === 0 && (
            <tr>
              <td colSpan={7} className="hint">
                No sessions yet — start a live chat to create one.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
