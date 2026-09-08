import { useState } from "react";
import { api, ApiError } from "../api/client";
import type { Agent, Companion, FaqCollection, KnowledgeBase } from "../api/types";

interface Props {
  companion: Companion | null;
  knowledgeBases: KnowledgeBase[];
  faqCollections: FaqCollection[];
  agent: Agent | null;
  onAgentCreated: (agent: Agent) => void;
}

export function AgentPanel({ companion, knowledgeBases, faqCollections, agent, onAgentCreated }: Props) {
  const [name, setName] = useState("Playground Agent");
  const [language, setLanguage] = useState("en-US");
  const [voiceId, setVoiceId] = useState("alloy");
  const [instructions, setInstructions] = useState("Be concise and friendly.");
  const [knowledgeBaseId, setKnowledgeBaseId] = useState("");
  const [selectedFaqs, setSelectedFaqs] = useState<string[]>([]);
  const [disableIdleTimeout, setDisableIdleTimeout] = useState(false);
  const [useWebSearch, setUseWebSearch] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function toggleFaq(id: string) {
    setSelectedFaqs((prev) => (prev.includes(id) ? prev.filter((f) => f !== id) : [...prev, id]));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!companion) return;
    setSubmitting(true);
    setError(null);
    try {
      const created = await api.createAgent({
        companionId: companion.id,
        name,
        language: language || undefined,
        voiceId: voiceId || undefined,
        providerSettings: { instructions },
        knowledgeBaseId: knowledgeBaseId || undefined,
        faqCollections: selectedFaqs.length > 0 ? selectedFaqs : undefined,
        disableIdleTimeout,
        useWebSearch,
      });
      onAgentCreated(created);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create agent.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!companion) {
    return <div className="panel"><p className="hint">Pick a companion first.</p></div>;
  }

  return (
    <div className="panel">
      <h2>Agent config for {companion.firstName} {companion.lastName}</h2>

      <form onSubmit={handleSubmit} className="form-grid">
        <label>
          Name
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>
        <label>
          Language
          <input value={language} onChange={(e) => setLanguage(e.target.value)} placeholder="en-US" />
        </label>
        <label>
          Voice ID
          <input value={voiceId} onChange={(e) => setVoiceId(e.target.value)} placeholder="alloy" />
        </label>
        <label className="form-grid__full">
          Instructions (providerSettings.instructions)
          <textarea
            value={instructions}
            onChange={(e) => setInstructions(e.target.value)}
            rows={4}
          />
        </label>
        <label>
          Knowledge base
          <select value={knowledgeBaseId} onChange={(e) => setKnowledgeBaseId(e.target.value)}>
            <option value="">None</option>
            {knowledgeBases.map((kb) => (
              <option key={kb.id} value={kb.id}>
                {kb.name}
              </option>
            ))}
          </select>
        </label>
        <div className="form-grid__full">
          FAQ collections
          <div className="checkbox-list">
            {faqCollections.map((f) => (
              <label key={f.id} className="checkbox-list__item">
                <input
                  type="checkbox"
                  checked={selectedFaqs.includes(f.id)}
                  onChange={() => toggleFaq(f.id)}
                />
                {f.name}
              </label>
            ))}
            {faqCollections.length === 0 && <span className="hint">None created yet.</span>}
          </div>
        </div>
        <label className="checkbox-list__item">
          <input
            type="checkbox"
            checked={disableIdleTimeout}
            onChange={(e) => setDisableIdleTimeout(e.target.checked)}
          />
          Disable idle timeout
        </label>
        <label className="checkbox-list__item">
          <input type="checkbox" checked={useWebSearch} onChange={(e) => setUseWebSearch(e.target.checked)} />
          Use web search
        </label>

        <div className="form-grid__full">
          <button type="submit" disabled={submitting}>
            {submitting ? "Creating…" : "Create agent"}
          </button>
        </div>
      </form>
      {error && <p className="error">{error}</p>}

      {agent && (
        <div className="subpanel">
          <h3>Current agent</h3>
          <table className="kv-table">
            <tbody>
              <tr><th>Id</th><td className="mono">{agent.id}</td></tr>
              <tr><th>Name</th><td>{agent.name}</td></tr>
              <tr><th>Voice</th><td>{agent.voiceId ?? "—"}</td></tr>
              <tr><th>Language</th><td>{agent.language ?? "—"}</td></tr>
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
