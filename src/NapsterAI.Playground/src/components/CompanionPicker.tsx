import { useEffect, useState } from "react";
import { api, ApiError } from "../api/client";
import type { Companion } from "../api/types";

interface Props {
  selectedCompanion: Companion | null;
  onSelect: (companion: Companion) => void;
}

function initials(firstName: string, lastName: string): string {
  const value = `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
  return value || "?";
}

/** Deterministic hue per companion id, so avatar placeholders stay stable and varied. */
function hueFromId(id: string): number {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) >>> 0;
  }
  return hash % 360;
}

const SKELETON_ROWS = 4;

export function CompanionPicker({ selectedCompanion, onSelect }: Props) {
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [gender, setGender] = useState("");
  const [companions, setCompanions] = useState<Companion[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Debounce free-text search so we're not firing a request per keystroke.
  useEffect(() => {
    const timeout = setTimeout(() => setSearch(searchInput.trim()), 300);
    return () => clearTimeout(timeout);
  }, [searchInput]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    api
      .browseCompanions({ search: search || undefined, gender: gender || undefined, pageSize: 30 })
      .then((result) => {
        if (!cancelled) setCompanions(result.items);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Failed to load companions.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [search, gender]);

  const hasFilters = search !== "" || gender !== "";

  function clearFilters() {
    setSearchInput("");
    setSearch("");
    setGender("");
  }

  return (
    <div className="companion-picker">
      <div className="companion-picker__toolbar">
        <label className="search-field">
          <svg className="search-field__icon" viewBox="0 0 20 20" fill="none" aria-hidden="true">
            <circle cx="9" cy="9" r="6" stroke="currentColor" strokeWidth="1.6" />
            <path d="M13.5 13.5L17 17" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
          </svg>
          <input
            type="search"
            placeholder="Search companions…"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
        </label>

        <div className="companion-picker__toolbar-row">
          <select value={gender} onChange={(e) => setGender(e.target.value)} className="pill-select">
            <option value="">Any gender</option>
            <option value="female">Female</option>
            <option value="male">Male</option>
            <option value="nonBinary">Non-binary</option>
          </select>

          {hasFilters && (
            <button type="button" className="link-button" onClick={clearFilters}>
              Clear
            </button>
          )}
        </div>
      </div>

      {error && <p className="error companion-picker__error">{error}</p>}

      <ul className="companion-picker__list">
        {loading &&
          companions.length === 0 &&
          Array.from({ length: SKELETON_ROWS }).map((_, i) => (
            <li key={`skeleton-${i}`} className="companion-card companion-card--skeleton" aria-hidden="true">
              <div className="skeleton skeleton--circle" />
              <div className="companion-card__meta">
                <div className="skeleton skeleton--line" style={{ width: "70%" }} />
                <div className="skeleton skeleton--line" style={{ width: "45%" }} />
              </div>
            </li>
          ))}

        {!loading &&
          companions.map((companion) => {
            const selected = selectedCompanion?.id === companion.id;
            return (
              <li key={companion.id}>
                <button
                  type="button"
                  className={`companion-card${selected ? " companion-card--selected" : ""}`}
                  onClick={() => onSelect(companion)}
                >
                  <div className="companion-card__avatar-wrap">
                    {companion.previewUrl ? (
                      <img src={companion.previewUrl} alt="" className="companion-card__avatar" />
                    ) : (
                      <div
                        className="companion-card__avatar companion-card__avatar--placeholder"
                        style={{ background: `hsl(${hueFromId(companion.id)} 68% 46%)` }}
                      >
                        {initials(companion.firstName, companion.lastName)}
                      </div>
                    )}
                    {companion.status === "ready" && <span className="companion-card__status-dot" title="Ready" />}
                  </div>

                  <div className="companion-card__meta">
                    <div className="companion-card__name-row">
                      <strong>
                        {companion.firstName} {companion.lastName}
                      </strong>
                      {selected && (
                        <svg className="companion-card__check" viewBox="0 0 20 20" fill="none" aria-hidden="true">
                          <path
                            d="M4 10.5L8 14.5L16 6"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                          />
                        </svg>
                      )}
                    </div>
                    {companion.headline && <span className="companion-card__headline">{companion.headline}</span>}
                    {(companion.gender || (companion.ethnicity && companion.ethnicity !== "unspecified")) && (
                      <div className="companion-card__chips">
                        {companion.gender && <span className="chip">{companion.gender}</span>}
                        {companion.ethnicity && companion.ethnicity !== "unspecified" && (
                          <span className="chip">{companion.ethnicity}</span>
                        )}
                      </div>
                    )}
                  </div>
                </button>
              </li>
            );
          })}

        {!loading && !error && companions.length === 0 && (
          <li className="companion-picker__empty">
            <p className="hint">No companions match your filters.</p>
          </li>
        )}
      </ul>
    </div>
  );
}
