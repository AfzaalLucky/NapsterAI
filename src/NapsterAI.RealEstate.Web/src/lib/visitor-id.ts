const STORAGE_KEY = "realestate-visitor-id";

// Napster's live validation rejects an externalClientId outside this shape - matches the same
// check NapsterService.CreateConnectionAsync enforces server-side (see LiveChatPanel.tsx).
const EXTERNAL_CLIENT_ID_PATTERN = /^[A-Za-z0-9_-]{1,32}$/;

function generateId(): string {
  const random = (crypto.randomUUID?.() ?? Math.random().toString(36).slice(2)).replace(/-/g, "");
  return `re-${random}`.slice(0, 32);
}

/**
 * A stable per-browser anonymous visitor id, used as the chat widget's externalClientId so a
 * returning visitor's sessions are attributable to the same client without requiring login.
 */
export function getVisitorId(): string {
  try {
    const existing = localStorage.getItem(STORAGE_KEY);
    if (existing && EXTERNAL_CLIENT_ID_PATTERN.test(existing)) {
      return existing;
    }
  } catch {
    // localStorage unavailable (private browsing, blocked storage) - fall through to a
    // session-only id below rather than failing the chat widget entirely.
  }

  const id = generateId();
  try {
    localStorage.setItem(STORAGE_KEY, id);
  } catch {
    // Ignore - the generated id still works for this page load, just won't persist.
  }
  return id;
}
