import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { useItems } from './useItems'

// Simple overview built from a single Items query, bucketed client-side. Fine
// for modest data volumes; promote to a dedicated aggregation endpoint if the
// item count grows large.
const OVERVIEW_PAGE_SIZE = 1000
const RECENT_LIMIT = 8

export function ItemsOverview() {
  const { data, isLoading } = useItems({
    page: 0,
    pageSize: OVERVIEW_PAGE_SIZE,
    sortBy: 'UpdatedOn',
    sortDescending: true,
  })

  const items = data?.items ?? []
  const total = data?.totalCount ?? 0
  const active = items.filter((i) => i.isActive).length
  const inactive = items.length - active
  const recent = items.slice(0, RECENT_LIMIT)

  return (
    <>
      <div className="grid gap-3 sm:grid-cols-3">
        <KpiCard label="Total items" value={total} loading={isLoading} />
        <KpiCard label="Active" value={active} loading={isLoading} />
        <KpiCard label="Inactive" value={inactive} loading={isLoading} />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recently updated</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-8 w-full" />
              ))}
            </div>
          ) : recent.length === 0 ? (
            <p className="text-muted-foreground text-sm">No items yet.</p>
          ) : (
            <ul className="divide-border divide-y">
              {recent.map((item) => (
                <li key={item.id}>
                  <Link
                    to={`/items/${item.id}`}
                    className="hover:bg-muted/40 flex items-center justify-between gap-3 rounded-md px-2 py-2"
                  >
                    <div className="min-w-0">
                      <p className="truncate font-medium">{item.name}</p>
                      <p className="text-muted-foreground truncate text-xs">{item.owner ?? '—'}</p>
                    </div>
                    <Badge
                      variant="secondary"
                      className={
                        item.isActive
                          ? 'bg-green-100 text-green-800 hover:bg-green-100'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-100'
                      }
                    >
                      {item.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </>
  )
}

function KpiCard({ label, value, loading }: { label: string; value: number; loading: boolean }) {
  return (
    <Card>
      <CardContent className="pt-6">
        <p className="text-accent text-[10px] font-semibold tracking-wider uppercase">{label}</p>
        {loading ? (
          <Skeleton className="mt-1 h-8 w-16" />
        ) : (
          <p className="mt-1 text-3xl font-semibold tabular-nums">{value}</p>
        )}
      </CardContent>
    </Card>
  )
}
