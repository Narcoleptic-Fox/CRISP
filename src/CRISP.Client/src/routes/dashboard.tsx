import type { ReactNode } from 'react'
//#if (IncludeSample)
import { ItemsOverview } from '@/features/items/items-overview'
//#endif

export function DashboardPage() {
  // Feature overviews slot in here. The sample contributes the Items overview; a solution
  // generated without the sample starts with a header and nothing else to say yet.
  let overview: ReactNode = null
  //#if (IncludeSample)
  overview = <ItemsOverview />
  //#endif

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
        <p className="text-foreground text-sm">An overview of what is in the catalog.</p>
      </header>
      {overview}
    </div>
  )
}
