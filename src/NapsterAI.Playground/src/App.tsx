import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "./api/client";
import type { Agent, Companion, FaqCollection, KnowledgeBase } from "./api/types";
import { CompanionPicker } from "./components/CompanionPicker";
import { AgentPanel } from "./components/AgentPanel";
import { KnowledgeBasePanel } from "./components/KnowledgeBasePanel";
import { FaqPanel } from "./components/FaqPanel";
import { SessionsPanel } from "./components/SessionsPanel";
import { LiveChatPanel } from "./components/LiveChatPanel";
import "./App.css";

const TABS = ["Agent", "Knowledge bases", "FAQs", "Live chat", "Sessions"] as const;
type Tab = (typeof TABS)[number];

function App() {
  const [companion, setCompanion] = useState<Companion | null>(null);
  const [agent, setAgent] = useState<Agent | null>(null);
  const [knowledgeBases, setKnowledgeBases] = useState<KnowledgeBase[]>([]);
  const [faqCollections, setFaqCollections] = useState<FaqCollection[]>([]);
  const [activeTab, setActiveTab] = useState<Tab>("Agent");
  const [loadError, setLoadError] = useState<string | null>(null);
  const [sessionsRefreshKey, setSessionsRefreshKey] = useState(0);

  const bumpSessionsRefresh = useCallback(() => setSessionsRefreshKey((key) => key + 1), []);

  const refreshKnowledgeBases = useCallback(() => {
    api
      .listKnowledgeBases({ pageSize: 100 })
      .then((result) => setKnowledgeBases(result.items))
      .catch((err: unknown) => setLoadError(err instanceof ApiError ? err.message : "Failed to load knowledge bases."));
  }, []);

  const refreshFaqCollections = useCallback(() => {
    api
      .listFaqCollections({ pageSize: 100 })
      .then((result) => setFaqCollections(result.items))
      .catch((err: unknown) => setLoadError(err instanceof ApiError ? err.message : "Failed to load FAQ collections."));
  }, []);

  useEffect(() => {
    refreshKnowledgeBases();
    refreshFaqCollections();
  }, [refreshKnowledgeBases, refreshFaqCollections]);

  function handleCompanionSelect(next: Companion) {
    setCompanion(next);
    setAgent(null);
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>NapsterAI Playground</h1>
        <p>Pick a companion, configure an agent, and chat with it live — backed by your own NapsterAI.Api.</p>
      </header>

      {loadError && <p className="error app-header__error">{loadError}</p>}

      <div className="app-body">
        <aside className="app-sidebar">
          <h2>Companions</h2>
          <CompanionPicker selectedCompanion={companion} onSelect={handleCompanionSelect} />
        </aside>

        <main className="app-main">
          <nav className="tabs">
            {TABS.map((tab) => (
              <button
                key={tab}
                type="button"
                className={`tabs__item${activeTab === tab ? " tabs__item--active" : ""}`}
                onClick={() => setActiveTab(tab)}
              >
                {tab}
              </button>
            ))}
          </nav>

          {/* Panels stay mounted so a live chat session survives switching tabs; only visibility toggles. */}
          <div style={{ display: activeTab === "Agent" ? "block" : "none" }}>
            <AgentPanel
              companion={companion}
              knowledgeBases={knowledgeBases}
              faqCollections={faqCollections}
              agent={agent}
              onAgentCreated={setAgent}
            />
          </div>
          <div style={{ display: activeTab === "Knowledge bases" ? "block" : "none" }}>
            <KnowledgeBasePanel knowledgeBases={knowledgeBases} onChanged={refreshKnowledgeBases} />
          </div>
          <div style={{ display: activeTab === "FAQs" ? "block" : "none" }}>
            <FaqPanel faqCollections={faqCollections} onChanged={refreshFaqCollections} />
          </div>
          <div style={{ display: activeTab === "Live chat" ? "block" : "none" }}>
            <LiveChatPanel companion={companion} agent={agent} onSessionActivity={bumpSessionsRefresh} />
          </div>
          <div style={{ display: activeTab === "Sessions" ? "block" : "none" }}>
            <SessionsPanel companion={companion} refreshKey={sessionsRefreshKey} />
          </div>
        </main>
      </div>
    </div>
  );
}

export default App;
