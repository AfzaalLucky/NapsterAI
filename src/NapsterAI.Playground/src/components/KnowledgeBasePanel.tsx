import { useState } from "react";
import { api, ApiError } from "../api/client";
import type { KnowledgeBase } from "../api/types";

const PROVIDERS = ["azureOpenAI", "gemini", "openAI", "humain", "microsoftFoundry"];

interface Props {
  knowledgeBases: KnowledgeBase[];
  onChanged: () => void;
}

export function KnowledgeBasePanel({ knowledgeBases, onChanged }: Props) {
  const [name, setName] = useState("");
  const [provider, setProvider] = useState(PROVIDERS[0]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await api.createKnowledgeBase({ name, provider });
      setName("");
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create knowledge base.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="panel">
      <h2>Knowledge bases</h2>
      <form className="form-row" onSubmit={handleSubmit}>
        <input
          placeholder="Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <select value={provider} onChange={(e) => setProvider(e.target.value)}>
          {PROVIDERS.map((p) => (
            <option key={p} value={p}>
              {p}
            </option>
          ))}
        </select>
        <button type="submit" disabled={submitting || !name.trim()}>
          {submitting ? "Creating…" : "Create"}
        </button>
      </form>
      {error && <p className="error">{error}</p>}

      <table className="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Provider</th>
            <th>Items</th>
            <th>Id</th>
          </tr>
        </thead>
        <tbody>
          {knowledgeBases.map((kb) => (
            <tr key={kb.id}>
              <td>{kb.name}</td>
              <td>{kb.provider ?? "—"}</td>
              <td>{kb.itemsCount ?? 0}</td>
              <td className="mono">{kb.id}</td>
            </tr>
          ))}
          {knowledgeBases.length === 0 && (
            <tr>
              <td colSpan={4} className="hint">
                No knowledge bases yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
