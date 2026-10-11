import type { ReactNode } from 'react'
import { TopNav } from './top-nav'

interface AppShellProps {
  children: ReactNode
}

export function AppShell({ children }: AppShellProps) {
  return (
    <div className="bg-background min-h-svh dark:bg-transparent">
      <TopNav />
      <main className="mx-auto w-full px-6 py-8">{children}</main>
    </div>
  )
}
