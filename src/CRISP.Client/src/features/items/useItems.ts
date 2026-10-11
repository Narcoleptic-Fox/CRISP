import { useQuery, keepPreviousData } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { Item } from '@/api/item'
import type { PagedResponse } from '@/api/paged-response'

export interface ItemFilters {
  name?: string
  owner?: string
  isActive?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

export const ITEMS_QUERY_KEY = ['items'] as const

function buildQueryString(filters: ItemFilters): string {
  const params = new URLSearchParams()
  params.set('page', String(filters.page ?? 0))
  params.set('pageSize', String(filters.pageSize ?? 100))
  if (filters.name) params.set('name', filters.name)
  if (filters.owner) params.set('owner', filters.owner)
  if (filters.isActive !== undefined) params.set('isActive', String(filters.isActive))
  if (filters.sortBy) params.set('sortBy', filters.sortBy)
  if (filters.sortDescending !== undefined)
    params.set('sortDescending', String(filters.sortDescending))
  return `?${params.toString()}`
}

export function useItems(filters: ItemFilters, enabled = true) {
  return useQuery({
    queryKey: [...ITEMS_QUERY_KEY, filters],
    queryFn: () => apiFetch<PagedResponse<Item>>(`/api/v1/Items${buildQueryString(filters)}`),
    staleTime: 30 * 1000,
    placeholderData: keepPreviousData,
    enabled,
  })
}
