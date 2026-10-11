/// <reference types="vite/client" />

interface ImportMetaEnv {
  /**
   * Redirect path (or full URL) used as the MSAL redirectUri. Must exactly
   * match a redirect URI registered on the Entra app registration by IT.
   * Optional; defaults to "/" when unset.
   */
  readonly VITE_REDIRECT_URI?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
