import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { loginRequest, msalInstance } from '@/auth/msalConfig'

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly body: unknown,
  ) {
    super(`API request failed with ${status}`)
  }
}

async function acquireAccessToken(): Promise<string> {
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
  if (!account) {
    await msalInstance.loginRedirect(loginRequest)
    throw new Error('Redirecting to sign in')
  }

  try {
    const result = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account,
    })
    return result.accessToken
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect(loginRequest)
      const redirectError = new Error('Redirecting to refresh token') as Error & { cause: unknown }
      redirectError.cause = error
      throw redirectError
    }
    throw error
  }
}

export async function apiFetch<TResponse>(
  path: string,
  init: RequestInit = {},
): Promise<TResponse> {
  const accessToken = await acquireAccessToken()

  const response = await fetch(path, {
    ...init,
    headers: {
      ...(init.headers ?? {}),
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new ApiError(response.status, body)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}
