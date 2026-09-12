import { useQuery, keepPreviousData } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { User } from '@/api/user'
import type { PagedResponse } from '@/api/paged-response'
import { Roles } from '@/api/roles'

export interface UserFilters {
  username?: string
  email?: string
  role?: Roles
  isActive?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

export const USERS_QUERY_KEY = ['identity', 'users'] as const

function buildQueryString(filters: UserFilters): string {
  const params = new URLSearchParams()
  params.set('page', String(filters.page ?? 0))
  params.set('pageSize', String(filters.pageSize ?? 50))
  if (filters.username) params.set('username', filters.username)
  if (filters.email) params.set('email', filters.email)
  if (filters.role !== undefined) params.set('role', String(filters.role))
  if (filters.isActive !== undefined) params.set('isActive', String(filters.isActive))
  if (filters.sortBy) params.set('sortBy', filters.sortBy)
  if (filters.sortDescending !== undefined)
    params.set('sortDescending', String(filters.sortDescending))
  return `?${params.toString()}`
}

export function useUsers(filters: UserFilters, enabled = true) {
  return useQuery({
    queryKey: [...USERS_QUERY_KEY, filters],
    queryFn: () => apiFetch<PagedResponse<User>>(`/api/v1/Users${buildQueryString(filters)}`),
    staleTime: 30 * 1000,
    placeholderData: keepPreviousData,
    enabled,
  })
}
