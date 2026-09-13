import type { ReactNode } from 'react'
import { AppShell } from '@/components/app-shell'

// No authentication: every route is open. This file replaces protected-route.tsx when the
// solution is generated with --auth none, so the router keeps its shape.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  return <AppShell>{children}</AppShell>
}
