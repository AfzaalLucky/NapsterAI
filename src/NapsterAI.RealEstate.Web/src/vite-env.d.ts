/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  /** Napster companion id for the "Real Estate AI Sales Agent" persona - see RealEstateChatWidget.tsx. */
  readonly VITE_REAL_ESTATE_COMPANION_ID?: string;
  readonly VITE_REAL_ESTATE_VOICE_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

declare module "@touchcastllc/napster-companion-api/styles";
