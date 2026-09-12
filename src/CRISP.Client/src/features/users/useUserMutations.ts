import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import { USERS_QUERY_KEY } from './useUsers'
import type { CreateUser } from '@/api/create-user'
import type { UpdateUser } from '@/api/update-user'

export function useCreateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: CreateUser) =>
      apiFetch<string>('/api/v1/Users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(command),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: USERS_QUERY_KEY })
    },
  })
}

export function useUpdateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: UpdateUser) =>
      apiFetch<void>('/api/v1/Users', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(command),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: USERS_QUERY_KEY })
    },
  })
}
