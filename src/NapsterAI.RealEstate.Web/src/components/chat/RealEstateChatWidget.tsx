import type { NapsterCompanionApiConfig, NapsterCompanionApiInstance } from "@touchcastllc/napster-companion-api";
import { DataChannelMessageType, NapsterCompanionApiSdk } from "@touchcastllc/napster-companion-api";
import { MessageCircleIcon, MicIcon, MicOffIcon, SendIcon, XIcon } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { ApiError, api } from "@/api/client";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getVisitorId } from "@/lib/visitor-id";

// EventMessage isn't re-exported from the package root, so derive its shape from the onData
// callback signature instead - same trick LiveChatPanel.tsx (NapsterAI.Playground) uses.
type OnDataMessage = Parameters<NonNullable<NapsterCompanionApiConfig["onData"]>>[0];

interface TranscriptEntry {
  id: number;
  role: string;
  text: string;
  final: boolean;
}

type Status = "idle" | "connecting" | "connected" | "error";

const COMPANION_ID = import.meta.env.VITE_REAL_ESTATE_COMPANION_ID;
const VOICE_ID = import.meta.env.VITE_REAL_ESTATE_VOICE_ID || "alloy";

/**
 * Floating AI chat widget for the public site, adapted from NapsterAI.Playground's
 * LiveChatPanel.tsx: same NapsterCompanionApiSdk.init() + POST /connections flow against a
 * fixed companion (the "Real Estate AI Sales Agent" persona - see .env.development for how
 * that's configured) instead of Playground's picker over every companion. EdgeMCP tool
 * registration (searchInventory, bookViewing, ...) is wired separately in Phase 9; the
 * EdgeMcpBridge that consumes it is already invoked automatically by SDK init() below.
 */
export function RealEstateChatWidget() {
  const [open, setOpen] = useState(false);
  const mountRef = useRef<HTMLDivElement>(null);
  const instanceRef = useRef<NapsterCompanionApiInstance | null>(null);
  const nextIdRef = useRef(0);

  const [status, setStatus] = useState<Status>("idle");
  const [error, setError] = useState<string | null>(null);
  const [transcript, setTranscript] = useState<TranscriptEntry[]>([]);
  const [draft, setDraft] = useState("");
  const [micMuted, setMicMuted] = useState(false);

  // Tear down any live session on unmount (e.g. client-side route change unmounting the layout).
  useEffect(() => {
    return () => {
      instanceRef.current?.destroy();
      instanceRef.current = null;
    };
  }, []);

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
    // The user's own text is already echoed locally by sendText() below - the server echoes it
    // back too (sometimes as more than one non-final chunk, which would otherwise concatenate
    // with itself via appendTranscript's merge logic), so skip re-appending the "user" role here.
    if (message?.content && message.role && message.role !== "user") {
      appendTranscript(message.role, message.content, message.action === "completed");
    }
  }

  async function startSession() {
    if (!COMPANION_ID) return;
    setStatus("connecting");
    setError(null);
    setTranscript([]);

    try {
      // Settle the mic prompt before minting the token - the connection's short lifetime
      // starts ticking the moment it's created.
      await NapsterCompanionApiSdk.requestMicrophoneAccess();

      const connection = await api.connections.create({
        companionId: COMPANION_ID,
        providerConfig: { voiceId: VOICE_ID },
        externalClientId: getVisitorId(),
      });

      if (!mountRef.current) throw new Error("Mount point not ready.");

      const instance = await NapsterCompanionApiSdk.init(connection.token, {
        mountContainer: mountRef.current,
        layout: "inline",
        onReady: () => setStatus("connected"),
        onError: (err) => {
          const detailed = err as Error & { code?: string };
          console.error("[RealEstateChatWidget] SDK error", err);
          setError(detailed.code ? `${err.message} (${detailed.code})` : err.message);
          setStatus("error");
        },
        onData: handleData,
        onDestroy: () => setStatus("idle"),
      });

      instanceRef.current = instance;
      setMicMuted(instance.isMicMuted);
    } catch (err) {
      setStatus("error");
      setError(
        err instanceof ApiError ? err.message : err instanceof Error ? err.message : "Failed to start the chat.",
      );
    }
  }

  function endSession() {
    instanceRef.current?.destroy();
    instanceRef.current = null;
    setStatus("idle");
    setTranscript([]);
  }

  function handleClose() {
    endSession();
    setOpen(false);
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

  // Not configured yet - stay invisible for visitors rather than showing a chat button that
  // can never connect. See .env.development for the setup runbook.
  if (!COMPANION_ID) {
    if (import.meta.env.DEV) {
      console.info(
        "[RealEstateChatWidget] VITE_REAL_ESTATE_COMPANION_ID is not set - the chat widget is hidden. See .env.development.",
      );
    }
    return null;
  }

  if (!open) {
    return (
      <Button
        onClick={() => setOpen(true)}
        size="icon"
        className="fixed right-6 bottom-6 z-50 size-14 rounded-full shadow-lg"
        aria-label="Chat with our AI assistant"
      >
        <MessageCircleIcon className="size-6" />
      </Button>
    );
  }

  return (
    <div className="bg-card fixed right-6 bottom-6 z-50 flex h-[32rem] w-[22rem] max-w-[calc(100vw-3rem)] flex-col overflow-hidden rounded-xl border shadow-2xl">
      <div className="flex items-center justify-between border-b px-4 py-3">
        <div>
          <p className="text-sm font-semibold">Real Estate AI Assistant</p>
          <p className="text-muted-foreground text-xs">
            {status === "connected" ? "Online" : status === "connecting" ? "Connecting…" : "Ask me anything"}
          </p>
        </div>
        <Button variant="ghost" size="icon" onClick={handleClose} aria-label="Close chat">
          <XIcon className="size-4" />
        </Button>
      </div>

      {/* Always rendered once the panel is open (even before status flips to "connected") so
          the SDK mounts into a container that already has real dimensions at init() time. */}
      <div ref={mountRef} className={cn("shrink-0", status === "idle" ? "h-0" : "h-32")} />

      <div className="flex-1 overflow-y-auto px-4 py-3">
        {(status === "idle" || status === "error") && transcript.length === 0 && (
          <div className="flex h-full flex-col items-center justify-center gap-3 text-center">
            <p className="text-muted-foreground text-sm">
              Ask about listings, pricing, or book a viewing - our AI assistant can help right now.
            </p>
            <Button onClick={startSession}>{status === "error" ? "Try again" : "Start chat"}</Button>
          </div>
        )}

        {status === "connecting" && (
          <p className="text-muted-foreground text-sm">Requesting mic access and connecting…</p>
        )}
        {error && <p className="text-destructive mt-2 text-sm">{error}</p>}

        <div className="flex flex-col gap-2">
          {transcript.map((entry) => (
            <div
              key={entry.id}
              className={cn(
                "max-w-[85%] rounded-lg px-3 py-2 text-sm",
                entry.role === "user" ? "bg-primary text-primary-foreground ml-auto" : "bg-muted",
              )}
            >
              {entry.text}
            </div>
          ))}
        </div>
      </div>

      {status === "connected" && (
        <div className="flex items-center gap-2 border-t p-3">
          <Button
            variant="ghost"
            size="icon"
            onClick={toggleMic}
            aria-label={micMuted ? "Unmute microphone" : "Mute microphone"}
          >
            {micMuted ? <MicOffIcon className="size-4" /> : <MicIcon className="size-4" />}
          </Button>
          <input
            className="border-input focus-visible:ring-ring/50 flex-1 rounded-md border bg-transparent px-3 py-1.5 text-sm outline-none focus-visible:ring-[3px]"
            placeholder="Type a message…"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && sendText()}
          />
          <Button size="icon" onClick={sendText} disabled={!draft.trim()} aria-label="Send message">
            <SendIcon className="size-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
