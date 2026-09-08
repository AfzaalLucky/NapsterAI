import { useState } from "react";
import { api, ApiError } from "../api/client";
import type { FaqCollection, FaqItem, FaqItemRequest } from "../api/types";

interface Props {
  faqCollections: FaqCollection[];
  onChanged: () => void;
}

const emptyPair = (): FaqItemRequest => ({ question: "", answer: "" });

export function FaqPanel({ faqCollections, onChanged }: Props) {
  const [name, setName] = useState("");
  const [pairs, setPairs] = useState<FaqItemRequest[]>([emptyPair()]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [viewingId, setViewingId] = useState<string | null>(null);
  const [items, setItems] = useState<FaqItem[]>([]);
  const [itemsError, setItemsError] = useState<string | null>(null);

  function updatePair(index: number, field: keyof FaqItemRequest, value: string) {
    setPairs((prev) => prev.map((p, i) => (i === index ? { ...p, [field]: value } : p)));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const faqs = pairs.filter((p) => p.question.trim() && p.answer.trim());
      await api.createFaqCollection({ name, faqs: faqs.length > 0 ? faqs : undefined });
      setName("");
      setPairs([emptyPair()]);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create FAQ collection.");
    } finally {
      setSubmitting(false);
    }
  }

  async function viewItems(id: string) {
    setViewingId(id);
    setItemsError(null);
    try {
      const result = await api.listFaqItems(id, { pageSize: 50 });
      setItems(result.items);
    } catch (err) {
      setItemsError(err instanceof ApiError ? err.message : "Failed to load FAQ items.");
    }
  }

  return (
    <div className="panel">
      <h2>FAQ collections</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-row">
          <input
            placeholder="Collection name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
        </div>

        {pairs.map((pair, i) => (
          <div className="form-row" key={i}>
            <input
              placeholder="Question"
              value={pair.question}
              onChange={(e) => updatePair(i, "question", e.target.value)}
            />
            <input
              placeholder="Answer"
              value={pair.answer}
              onChange={(e) => updatePair(i, "answer", e.target.value)}
            />
          </div>
        ))}
        <div className="form-row">
          <button type="button" onClick={() => setPairs((prev) => [...prev, emptyPair()])}>
            + Add pair
          </button>
          <button type="submit" disabled={submitting || !name.trim()}>
            {submitting ? "Creating…" : "Create collection"}
          </button>
        </div>
      </form>
      {error && <p className="error">{error}</p>}

      <table className="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Items</th>
            <th>Id</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {faqCollections.map((c) => (
            <tr key={c.id}>
              <td>{c.name}</td>
              <td>{c.itemsCount ?? 0}</td>
              <td className="mono">{c.id}</td>
              <td>
                <button type="button" onClick={() => viewItems(c.id)}>
                  View items
                </button>
              </td>
            </tr>
          ))}
          {faqCollections.length === 0 && (
            <tr>
              <td colSpan={4} className="hint">
                No FAQ collections yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {viewingId && (
        <div className="subpanel">
          <h3>Items in {viewingId}</h3>
          {itemsError && <p className="error">{itemsError}</p>}
          <ul>
            {items.map((item) => (
              <li key={item.id}>
                <strong>{item.question}</strong>
                <div>{item.answer}</div>
              </li>
            ))}
            {items.length === 0 && !itemsError && <p className="hint">No items.</p>}
          </ul>
        </div>
      )}
    </div>
  );
}
