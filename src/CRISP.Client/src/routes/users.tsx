import { useState } from 'react'
import { PlusIcon, SearchIcon } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHeader, TableRow } from '@/components/ui/table'
import { SortableTableHead, type SortState } from '@/components/sortable-table-head'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { cn } from '@/lib/utils'
import { Roles } from '@/api/roles'
import type { User } from '@/api/user'
import { useUsers, type UserFilters } from '@/features/users/useUsers'
import { UserFormDialog } from '@/features/users/user-form-dialog'

const ROLE_LABELS: Record<Roles, string> = {
  [Roles.Viewer]: 'Viewer',
  [Roles.Editor]: 'Editor',
  [Roles.Admin]: 'Admin',
}

const ROLE_BADGE: Record<Roles, string> = {
  [Roles.Viewer]: 'bg-gray-100 text-gray-700 hover:bg-gray-100',
  [Roles.Editor]: 'bg-blue-100 text-blue-800 hover:bg-blue-100',
  [Roles.Admin]: 'bg-violet-100 text-violet-800 hover:bg-violet-100',
}

const ROLE_FILTER_ALL = 'all'
const ACTIVE_FILTER_ALL = 'all'
const ACTIVE_FILTER_ACTIVE = 'active'
const ACTIVE_FILTER_INACTIVE = 'inactive'
const PAGE_SIZE = 100

export function UsersPage() {
  const { data: currentUser, isLoading: loadingCurrent } = useCurrentUser()

  if (loadingCurrent) {
    return <p className="text-muted-foreground text-sm">Loading…</p>
  }

  if (currentUser?.user?.role !== Roles.Admin) {
    return <ForbiddenView />
  }

  return <UsersPageContent />
}

function ForbiddenView() {
  return (
    <div className="space-y-2">
      <h1 className="text-2xl font-semibold tracking-tight">Forbidden</h1>
      <p className="text-foreground text-sm">
        You do not have permission to view this page. Contact an administrator if you believe this
        is an error.
      </p>
    </div>
  )
}

function UsersPageContent() {
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState<string>(ROLE_FILTER_ALL)
  const [activeFilter, setActiveFilter] = useState<string>(ACTIVE_FILTER_ALL)
  const [page, setPage] = useState(0)
  const [sort, setSort] = useState<SortState>({})
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingUser, setEditingUser] = useState<User | null>(null)

  const handleSearchChange = (value: string) => {
    setSearch(value)
    setPage(0)
  }
  const handleRoleFilterChange = (value: string) => {
    setRoleFilter(value)
    setPage(0)
  }
  const handleActiveFilterChange = (value: string) => {
    setActiveFilter(value)
    setPage(0)
  }
  const handleSortChange = (next: SortState) => {
    setSort(next)
    setPage(0)
  }

  const filters: UserFilters = {
    username: search.trim() || undefined,
    role: roleFilter === ROLE_FILTER_ALL ? undefined : (Number(roleFilter) as Roles),
    isActive:
      activeFilter === ACTIVE_FILTER_ALL ? undefined : activeFilter === ACTIVE_FILTER_ACTIVE,
    page,
    pageSize: PAGE_SIZE,
    sortBy: sort.sortBy,
    sortDescending: sort.sortDescending,
  }

  const { data, isLoading, isFetching, error } = useUsers(filters)
  const users = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))
  const firstRow = totalCount === 0 ? 0 : page * PAGE_SIZE + 1
  const lastRow = Math.min(totalCount, (page + 1) * PAGE_SIZE)

  const handleOpenCreate = () => {
    setEditingUser(null)
    setDialogOpen(true)
  }

  const handleOpenEdit = (user: User) => {
    setEditingUser(user)
    setDialogOpen(true)
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
          <p className="text-foreground text-sm">
            Provision, edit, and deactivate application accounts.
          </p>
        </div>
        <Button onClick={handleOpenCreate}>
          <PlusIcon className="size-4" />
          New user
        </Button>
      </header>

      <Card>
        <CardContent className="space-y-4 pt-6">
          <div className="flex flex-wrap items-center gap-3">
            <div className="relative min-w-[240px] flex-1">
              <SearchIcon className="text-muted-foreground absolute top-1/2 left-2.5 size-4 -translate-y-1/2" />
              <Input
                value={search}
                onChange={(event) => handleSearchChange(event.target.value)}
                placeholder="Search by username…"
                className="pl-8"
              />
            </div>
            <Select value={roleFilter} onValueChange={handleRoleFilterChange}>
              <SelectTrigger className="w-36">
                <SelectValue />
              </SelectTrigger>
              <SelectContent position="popper">
                <SelectItem value={ROLE_FILTER_ALL}>All roles</SelectItem>
                <SelectItem value={String(Roles.Viewer)}>Viewer</SelectItem>
                <SelectItem value={String(Roles.Editor)}>Editor</SelectItem>
                <SelectItem value={String(Roles.Admin)}>Admin</SelectItem>
              </SelectContent>
            </Select>
            <Select value={activeFilter} onValueChange={handleActiveFilterChange}>
              <SelectTrigger className="w-36">
                <SelectValue />
              </SelectTrigger>
              <SelectContent position="popper">
                <SelectItem value={ACTIVE_FILTER_ALL}>All statuses</SelectItem>
                <SelectItem value={ACTIVE_FILTER_ACTIVE}>Active</SelectItem>
                <SelectItem value={ACTIVE_FILTER_INACTIVE}>Inactive</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {error ? (
            <p className="text-destructive text-sm">
              Could not load users. Please refresh the page.
            </p>
          ) : (
            <Table containerClassName="h-[60vh] rounded-md border">
              <TableHeader className="bg-card sticky top-0 z-10 shadow-[inset_0_-1px_0_var(--color-border)]">
                <TableRow>
                  <SortableTableHead column="Username" sort={sort} onSortChange={handleSortChange}>
                    Username
                  </SortableTableHead>
                  <SortableTableHead column="Email" sort={sort} onSortChange={handleSortChange}>
                    Email
                  </SortableTableHead>
                  <SortableTableHead column="Role" sort={sort} onSortChange={handleSortChange}>
                    Role
                  </SortableTableHead>
                  <SortableTableHead column="IsActive" sort={sort} onSortChange={handleSortChange}>
                    Status
                  </SortableTableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  Array.from({ length: 5 }).map((_, idx) => (
                    <TableRow key={idx}>
                      {Array.from({ length: 5 }).map((__, col) => (
                        <TableCell key={col}>
                          <Skeleton className="h-4 w-full max-w-[180px]" />
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : users.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} className="text-muted-foreground text-center text-sm">
                      No users match the current filters.
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
                    <TableRow
                      key={user.id}
                      className="hover:bg-muted/40 cursor-pointer"
                      onClick={() => handleOpenEdit(user)}
                    >
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell className="text-muted-foreground">{user.email}</TableCell>
                      <TableCell>
                        <Badge
                          variant="secondary"
                          className={cn('font-semibold', ROLE_BADGE[user.role])}
                        >
                          {ROLE_LABELS[user.role] ?? 'Unknown'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant="secondary"
                          className={
                            user.isActive
                              ? 'bg-green-100 text-green-800 hover:bg-green-100'
                              : 'bg-gray-100 text-gray-700 hover:bg-gray-100'
                          }
                        >
                          {user.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}

          <div className="flex flex-wrap items-center justify-between gap-3 pt-2">
            <p className="text-muted-foreground text-xs tabular-nums">
              {totalCount === 0
                ? isLoading
                  ? 'Loading…'
                  : 'No results'
                : `Showing ${firstRow}–${lastRow} of ${totalCount}`}
              {!isLoading && isFetching && totalCount > 0 ? ' · refreshing…' : ''}
            </p>
            {pageCount > 1 && (
              <UsersPagination page={page} pageCount={pageCount} onPageChange={setPage} />
            )}
          </div>
        </CardContent>
      </Card>

      <UserFormDialog open={dialogOpen} onOpenChange={setDialogOpen} user={editingUser} />
    </div>
  )
}

interface UsersPaginationProps {
  page: number
  pageCount: number
  onPageChange: (page: number) => void
}

// Build a compact 5-slot page list: always show first and last, the current page,
// its neighbors, and collapse the rest into ellipses.
function buildPageItems(page: number, pageCount: number): (number | 'ellipsis')[] {
  if (pageCount <= 7) {
    return Array.from({ length: pageCount }, (_, i) => i)
  }
  const items: (number | 'ellipsis')[] = [0]
  const start = Math.max(1, page - 1)
  const end = Math.min(pageCount - 2, page + 1)
  if (start > 1) items.push('ellipsis')
  for (let i = start; i <= end; i++) items.push(i)
  if (end < pageCount - 2) items.push('ellipsis')
  items.push(pageCount - 1)
  return items
}

function UsersPagination({ page, pageCount, onPageChange }: UsersPaginationProps) {
  const items = buildPageItems(page, pageCount)
  const goTo = (next: number) => {
    const clamped = Math.max(0, Math.min(pageCount - 1, next))
    if (clamped !== page) onPageChange(clamped)
  }

  return (
    <Pagination className="mx-0 w-auto justify-end">
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            href="#"
            aria-disabled={page === 0}
            className={page === 0 ? 'pointer-events-none opacity-50' : ''}
            onClick={(event) => {
              event.preventDefault()
              goTo(page - 1)
            }}
          />
        </PaginationItem>
        {items.map((item, idx) =>
          item === 'ellipsis' ? (
            <PaginationItem key={`ellipsis-${idx}`}>
              <PaginationEllipsis />
            </PaginationItem>
          ) : (
            <PaginationItem key={item}>
              <PaginationLink
                href="#"
                isActive={item === page}
                onClick={(event) => {
                  event.preventDefault()
                  goTo(item)
                }}
              >
                {item + 1}
              </PaginationLink>
            </PaginationItem>
          ),
        )}
        <PaginationItem>
          <PaginationNext
            href="#"
            aria-disabled={page >= pageCount - 1}
            className={page >= pageCount - 1 ? 'pointer-events-none opacity-50' : ''}
            onClick={(event) => {
              event.preventDefault()
              goTo(page + 1)
            }}
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  )
}
