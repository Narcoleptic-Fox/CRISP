import { useQuery } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { Item } from '@/api/item'
import { ITEMS_QUERY_KEY } from './useItems'

export function useItem(id: string | undefined) {
  return useQuery({
    queryKey: [...ITEMS_QUERY_KEY, 'detail', id],
    queryFn: () => apiFetch<Item>(`/api/v1/Items/${id}`),
    enabled: !!id,
  })
}
