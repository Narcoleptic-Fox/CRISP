import { useQuery } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { AutoComplete } from '@/api/auto-complete'

// Distinct owner names are sourced from existing items to drive a free-text
// autocomplete. They change rarely, so cache aggressively.
export const ITEM_OWNER_NAMES_QUERY_KEY = ['items', 'owner-names'] as const

export function useItemOwnerNames() {
  return useQuery({
    queryKey: ITEM_OWNER_NAMES_QUERY_KEY,
    queryFn: () => apiFetch<AutoComplete[]>('/api/v1/Items/owners'),
    staleTime: 5 * 60 * 1000,
  })
}
