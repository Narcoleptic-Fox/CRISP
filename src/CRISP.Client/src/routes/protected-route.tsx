import type { ReactNode } from 'react'
import { AppShell } from '@/components/app-shell'
import { AuthGate } from '@/auth/AuthGate'

export function ProtectedRoute({ children }: { children: ReactNode }) {
  return (
    <AuthGate>
      <AppShell>{children}</AppShell>
    </AuthGate>
  )
}
