import { useQuery } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { AutoComplete } from '@/api/auto-complete'

// Name lookups change rarely; cache aggressively so combobox open is instant.
const NAMES_STALE_TIME = 5 * 60 * 1000

export const USER_NAMES_QUERY_KEY = ['identity', 'user-names'] as const

export function useUserNames() {
  return useQuery({
    queryKey: USER_NAMES_QUERY_KEY,
    queryFn: () => apiFetch<AutoComplete[]>('/api/v1/Users/names'),
    staleTime: NAMES_STALE_TIME,
  })
}

// Looks up an option's ID by display name. Useful for seeding edit forms from a
// view model that only carries the resolved name string. Returns null when
// there's no exact match (renamed, deleted, or never existed).
export function findIdByName(
  options: AutoComplete[] | undefined,
  name: string | null | undefined,
): string | null {
  if (!name || !options) return null
  const match = options.find((o) => o.name === name)
  return match ? match.id : null
}
