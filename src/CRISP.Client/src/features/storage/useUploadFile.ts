import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'
import type { CreatePresignedDownloadUrl } from '@/api/create-presigned-download-url'
import type { CreatePresignedUploadUrl } from '@/api/create-presigned-upload-url'
import type { PresignedUrl } from '@/api/presigned-url'
import { ITEMS_QUERY_KEY } from '@/features/items/useItems'

// The Core entity type name (without the "Entity" suffix) that owns the files.
// The generic Storage endpoints resolve the entity by reflection over
// IHasStoredFiles implementations, so any file-bearing entity works the same way.
const ENTITY_TYPE = 'Item'

export interface UploadFileInput {
  entityType: string
  entityId: string
  file: File
}

// Two-step upload: the server returns a presigned URL and records the file
// metadata; the client then PUTs the bytes directly to object storage.
async function upload({ entityType, entityId, file }: UploadFileInput): Promise<void> {
  const command: CreatePresignedUploadUrl = {
    entityType,
    entityId,
    originalFileName: file.name,
    contentType: file.type || 'application/octet-stream',
    sizeBytes: file.size,
    expiresInMinutes: 15,
  }
  const presigned = await apiFetch<PresignedUrl>('/api/v1/Storage/upload', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(command),
  })

  // An empty url means the server's storage provider is the no-op stub — the
  // metadata row was still recorded, so surface success without PUTting.
  if (!presigned.url) return

  const response = await fetch(presigned.url, {
    method: presigned.method,
    headers: presigned.requiredHeaders,
    body: file,
  })
  if (!response.ok) {
    throw new Error(`Upload failed (${response.status})`)
  }
}

export interface DownloadFileInput {
  entityType: string
  entityId: string
  fileId: string
  fileName: string
}

// The server returns a presigned GET URL pointing at object storage. We trigger
// the browser download by clicking a hidden anchor carrying the original
// filename so saved files keep their human-readable name.
async function download({
  entityType,
  entityId,
  fileId,
  fileName,
}: DownloadFileInput): Promise<void> {
  const command: CreatePresignedDownloadUrl = {
    entityType,
    entityId,
    fileId,
    expiresInMinutes: 15,
  }
  const presigned = await apiFetch<PresignedUrl>('/api/v1/Storage/download', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(command),
  })

  if (!presigned.url) {
    throw new Error('Storage provider is disabled — no download URL available.')
  }

  const anchor = document.createElement('a')
  anchor.href = presigned.url
  anchor.download = fileName
  anchor.rel = 'noopener'
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
}

export function useDownloadItemFile() {
  return useMutation({
    mutationFn: (input: Omit<DownloadFileInput, 'entityType'>) =>
      download({ entityType: ENTITY_TYPE, ...input }),
  })
}

export function useUploadItemFiles() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ itemId, files }: { itemId: string; files: File[] }) => {
      for (const file of files) {
        await upload({ entityType: ENTITY_TYPE, entityId: itemId, file })
      }
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ITEMS_QUERY_KEY })
    },
  })
}
