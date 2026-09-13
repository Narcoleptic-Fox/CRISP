import {
  PublicClientApplication,
  type Configuration,
  type RedirectRequest,
} from '@azure/msal-browser'

const instance = 'https://login.microsoftonline.com'
const tenantId = '254ba93e-1f6f-48f3-90e6-e2766664b477'
const clientId = '76818d5d-5aec-42a0-9d90-ec7439054f51'
// Entra requires the redirect URI to exactly match a value registered on the
// app registration. The origin is taken from the running site so a single
// build works across dev/staging/prod; only the path differs and is dictated
// by what IT registered for this app (some apps use /signin-oidc,
// others the site root). Override via VITE_REDIRECT_URI (a path, e.g.
// "/signin-oidc"); defaults to "/" when unset. new URL() also accepts a full
// absolute URL here and uses it verbatim.
const configuredRedirect = import.meta.env.VITE_REDIRECT_URI?.trim() || '/'
const redirectUri = new URL(configuredRedirect, window.location.origin).toString()

export const apiScopes: string[] = [`${clientId}/.default`]

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `${instance}/${tenantId}`,
    redirectUri,
    postLogoutRedirectUri: null,
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
}

export const msalInstance = new PublicClientApplication(msalConfig)

export const loginRequest: RedirectRequest = {
  scopes: apiScopes,
}
