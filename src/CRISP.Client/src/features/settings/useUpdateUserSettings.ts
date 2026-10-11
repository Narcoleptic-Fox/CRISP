import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import { CURRENT_USER_QUERY_KEY } from '@/auth/useCurrentUser'
import type { UpdateUserSettings } from '@/api/update-user-settings'

export function useUpdateUserSettings() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (command: UpdateUserSettings) =>
      apiFetch<void>('/api/v1/UserSettings', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(command),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY })
    },
  })
}
