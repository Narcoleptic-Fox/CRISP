import { ArrowDownIcon, ArrowUpDownIcon, ArrowUpIcon } from 'lucide-react'
import { TableHead } from '@/components/ui/table'
import { cn } from '@/lib/utils'

export interface SortState {
  sortBy?: string
  sortDescending?: boolean
}

interface SortableTableHeadProps {
  children: React.ReactNode
  // Server-side property name (PascalCase matching the entity property).
  column: string
  sort: SortState
  onSortChange: (next: SortState) => void
  className?: string
}

// Click cycles: unsorted -> asc -> desc -> unsorted.
function nextSort(current: SortState, column: string): SortState {
  if (current.sortBy !== column) return { sortBy: column, sortDescending: false }
  if (current.sortDescending === false) return { sortBy: column, sortDescending: true }
  return { sortBy: undefined, sortDescending: undefined }
}

export function SortableTableHead({
  children,
  column,
  sort,
  onSortChange,
  className,
}: SortableTableHeadProps) {
  const isActive = sort.sortBy === column
  const ariaSort = isActive ? (sort.sortDescending ? 'descending' : 'ascending') : 'none'
  const Icon = isActive ? (sort.sortDescending ? ArrowDownIcon : ArrowUpIcon) : ArrowUpDownIcon

  return (
    <TableHead aria-sort={ariaSort} className={className}>
      <button
        type="button"
        onClick={() => onSortChange(nextSort(sort, column))}
        className={cn(
          'group hover:bg-muted/60 -ml-1 inline-flex items-center gap-1 rounded px-1 py-0.5',
          isActive ? 'text-foreground' : 'text-muted-foreground',
        )}
      >
        <span>{children}</span>
        <Icon
          className={cn('size-3.5', isActive ? 'opacity-100' : 'opacity-40 group-hover:opacity-80')}
        />
      </button>
    </TableHead>
  )
}
