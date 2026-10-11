import type { ReactNode } from 'react'
import { InteractionType } from '@azure/msal-browser'
import { MsalAuthenticationTemplate } from '@azure/msal-react'
import { Navigate } from 'react-router-dom'
import { FullPageLoader } from '@/components/full-page-loader'
import { loginRequest } from './msalConfig'
import { useCurrentUser } from './useCurrentUser'

interface AuthGateProps {
  children: ReactNode
}

function SigningInPlaceholder() {
  return <FullPageLoader message="Signing you in..." />
}

function SignInError() {
  return (
    <main className="flex min-h-svh items-center justify-center">
      <p className="text-destructive text-sm">
        Sign-in failed. Please refresh the page to try again.
      </p>
    </main>
  )
}

function ProvisioningGate({ children }: { children: ReactNode }) {
  const { data, isLoading, error } = useCurrentUser()

  if (isLoading) {
    return <FullPageLoader message="Loading your account..." />
  }

  if (error) {
    return (
      <main className="flex min-h-svh items-center justify-center">
        <p className="text-destructive text-sm">
          Could not load your account. Please refresh the page.
        </p>
      </main>
    )
  }

  if (data?.status === 'unprovisioned' || data?.status === 'deactivated') {
    return <Navigate to="/no-access" replace />
  }

  return <>{children}</>
}

export function AuthGate({ children }: AuthGateProps) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      loadingComponent={SigningInPlaceholder}
      errorComponent={SignInError}
    >
      <ProvisioningGate>{children}</ProvisioningGate>
    </MsalAuthenticationTemplate>
  )
}
