import { useQuery } from '@tanstack/react-query'
import { ApiError, apiFetch } from '@/lib/api'
import { Roles } from '@/api/roles'
import type { User } from '@/api/user'

export const CURRENT_USER_QUERY_KEY = ['identity', 'me'] as const

export interface CurrentUserResult {
  status: 'authorized' | 'unprovisioned' | 'deactivated'
  user: User | null
}

async function fetchCurrentUser(): Promise<CurrentUserResult> {
  try {
    const user = await apiFetch<User>('/api/v1/Users/me')
    return {
      status: user.isActive ? 'authorized' : 'deactivated',
      user,
    }
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return { status: 'unprovisioned', user: null }
    }
    throw error
  }
}

export function useCurrentUser() {
  return useQuery({
    queryKey: CURRENT_USER_QUERY_KEY,
    queryFn: fetchCurrentUser,
    staleTime: 5 * 60 * 1000,
    retry: false,
  })
}

// Gates write-side UI (create/edit/retire/upload). Returns false while the
// current user is loading or for any role below Editor, so the UI fails
// closed if the request hasn't resolved yet.
export function useCanWrite(): boolean {
  const { data } = useCurrentUser()
  const role = data?.user?.role
  return role === Roles.Editor || role === Roles.Admin
}
