import { useEffect, useRef, useState } from "react";
import { NapsterCompanionApiSdk, DataChannelMessageType } from "@touchcastllc/napster-companion-api";
import type { NapsterCompanionApiInstance, NapsterCompanionApiConfig } from "@touchcastllc/napster-companion-api";
import { api, ApiError } from "../api/client";
import type { Agent, Companion } from "../api/types";

// EventMessage isn't re-exported from the package root, so derive its shape
// from the onData callback signature instead of importing it directly.
type OnDataMessage = Parameters<NonNullable<NapsterCompanionApiConfig["onData"]>>[0];

// Not documented, but Napster's live validation rejects an externalClientId outside this
// shape — matches the same check NapsterService.CreateConnectionAsync enforces server-side.
const EXTERNAL_CLIENT_ID_PATTERN = /^[A-Za-z0-9_-]{1,32}$/;

interface TranscriptEntry {
  id: number;
  role: string;
  text: string;
  final: boolean;
}

interface Props {
  companion: Companion | null;
  agent: Agent | null;
  /** Called when a session starts or ends, so the Sessions tab can refetch. */
  onSessionActivity?: () => void;
}

type Status = "idle" | "connecting" | "connected" | "error";

export function LiveChatPanel({ companion, agent, onSessionActivity }: Props) {
  const mountRef = useRef<HTMLDivElement>(null);
  const instanceRef = useRef<NapsterCompanionApiInstance | null>(null);
  const nextIdRef = useRef(0);

  const [voiceId, setVoiceId] = useState("alloy");
  const [externalClientId, setExternalClientId] = useState("");
  const [status, setStatus] = useState<Status>("idle");
  const [error, setError] = useState<string | null>(null);
  const [transcript, setTranscript] = useState<TranscriptEntry[]>([]);
  const [draft, setDraft] = useState("");
  const [micMuted, setMicMuted] = useState(false);

  useEffect(() => {
    if (agent?.voiceId) setVoiceId(agent.voiceId);
  }, [agent]);

  // Tear down any live session when the selected companion changes or the panel unmounts.
  useEffect(() => {
    return () => {
      instanceRef.current?.destroy();
      instanceRef.current = null;
    };
  }, [companion?.id]);

  function appendTranscript(role: string, text: string, final: boolean) {
    setTranscript((prev) => {
      const last = prev[prev.length - 1];
      if (last && last.role === role && !last.final) {
        const updated = [...prev];
        updated[updated.length - 1] = { ...last, text: last.text + text, final };
        return updated;
      }
      return [...prev, { id: nextIdRef.current++, role, text, final }];
    });
  }

  function handleData(msg: OnDataMessage) {
    const message = msg.data?.message;
    if (message?.content && message.role) {
      appendTranscript(message.role, message.content, message.action === "completed");
    }
  }

  const externalClientIdError =
    externalClientId && !EXTERNAL_CLIENT_ID_PATTERN.test(externalClientId)
      ? "Only letters, digits, hyphens (-), and underscores (_), max 32 characters."
      : null;

  async function startSession() {
    if (!companion || externalClientIdError) return;
    setStatus("connecting");
    setError(null);
    setTranscript([]);

    try {
      // Settle the mic prompt before minting the token — the connection's short
      // lifetime starts ticking the moment it's created.
      await NapsterCompanionApiSdk.requestMicrophoneAccess();

      const connection = await api.createConnection({
        companionId: companion.id,
        providerConfig: { voiceId },
        externalClientId: externalClientId || undefined,
      });

      if (!mountRef.current) throw new Error("Mount point not ready.");

      const instance = await NapsterCompanionApiSdk.init(connection.token, {
        mountContainer: mountRef.current,
        layout: "inline",
        debug: true,
        onReady: () => {
          setStatus("connected");
          // Napster records the session server-side once the connection is live.
          onSessionActivity?.();
        },
        onError: (err) => {
          // The SDK's real error classes (WebRTCError, ConnectionError, ...) carry a `code`
          // and a `context` object with much more detail than `message` alone, but aren't
          // re-exported from the package's public types — read them defensively at runtime.
          const detailed = err as Error & { code?: string; context?: Record<string, unknown> };
          console.error("[NapsterAI Playground] SDK error", {
            code: detailed.code,
            context: detailed.context,
            error: err,
          });
          setError(detailed.code ? `${err.message} (${detailed.code})` : err.message);
          setStatus("error");
        },
        onData: handleData,
        onDestroy: () => {
          setStatus("idle");
          // Cost/status/closedAt only settle once the session actually closes upstream.
          onSessionActivity?.();
        },
      });

      instanceRef.current = instance;
      setMicMuted(instance.isMicMuted);
    } catch (err) {
      setStatus("error");
      setError(
        err instanceof ApiError
          ? err.message
          : err instanceof Error
            ? err.message
            : "Failed to start the session.",
      );
    }
  }

  function endSession() {
    instanceRef.current?.destroy();
    instanceRef.current = null;
    setStatus("idle");
  }

  function sendText() {
    const text = draft.trim();
    const instance = instanceRef.current;
    if (!text || !instance) return;
    instance.sendCommand({
      type: DataChannelMessageType.SEND_MESSAGE,
      data: { text, role: "user", trigger_response: true },
    });
    appendTranscript("user", text, true);
    setDraft("");
  }

  function toggleMic() {
    const instance = instanceRef.current;
    if (!instance) return;
    if (instance.isMicMuted) {
      instance.unmuteMic();
    } else {
      instance.muteMic();
    }
    setMicMuted(instance.isMicMuted);
  }

  if (!companion) {
    return (
      <div className="panel">
        <p className="hint">Pick a companion first.</p>
      </div>
    );
  }

  return (
    <div className="panel live-chat">
      <h2>
        Live chat with {companion.firstName} {companion.lastName}
      </h2>

      {status === "idle" && (
        <>
          <div className="form-row">
            <input placeholder="Voice ID" value={voiceId} onChange={(e) => setVoiceId(e.target.value)} />
            <input
              placeholder="External client id (optional)"
              value={externalClientId}
              onChange={(e) => setExternalClientId(e.target.value)}
              maxLength={32}
              aria-invalid={externalClientIdError != null}
            />
            <button type="button" onClick={startSession} disabled={externalClientIdError != null}>
              Start session
            </button>
          </div>
          {externalClientIdError && <p className="error">{externalClientIdError}</p>}
        </>
      )}

      {status === "connecting" && <p className="hint">Requesting mic access and connecting…</p>}
      {error && <p className="error">{error}</p>}

      <div className="live-chat__body">
        <div ref={mountRef} className="live-chat__avatar" />

        <div className="live-chat__transcript">
          {transcript.map((entry) => (
            <div key={entry.id} className={`transcript-entry transcript-entry--${entry.role}`}>
              <strong>{entry.role}</strong>
              <span>{entry.text}</span>
            </div>
          ))}
          {transcript.length === 0 && <p className="hint">No messages yet.</p>}
        </div>
      </div>

      {status === "connected" && (
        <div className="form-row">
          <input
            placeholder="Type a message…"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && sendText()}
          />
          <button type="button" onClick={sendText} disabled={!draft.trim()}>
            Send
          </button>
          <button type="button" onClick={toggleMic}>
            {micMuted ? "Unmute mic" : "Mute mic"}
          </button>
          <button type="button" onClick={endSession}>
            End session
          </button>
        </div>
      )}
    </div>
  );
}
