import { useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import {
  ArrowLeftIcon,
  DownloadIcon,
  FileTextIcon,
  PencilIcon,
  PlusIcon,
  Trash2Icon,
  UploadCloudIcon,
  XIcon,
} from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useCanWrite } from '@/auth/useCurrentUser'
import { useItem } from '@/features/items/useItem'
import { useDeleteItem } from '@/features/items/useItemMutations'
import { ItemFormDialog } from '@/features/items/item-form-dialog'
import { useDownloadItemFile, useUploadItemFiles } from '@/features/storage/useUploadFile'
import { ApiError } from '@/lib/api'
import type { Item } from '@/api/item'
import { cn } from '@/lib/utils'

function formatDate(value: string | Date | null | undefined): string {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('en-US')
}

export function ItemDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: item, isLoading, error } = useItem(id)

  const [editOpen, setEditOpen] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [uploadOpen, setUploadOpen] = useState(false)

  if (isLoading) return <ItemDetailSkeleton />
  if (error || !item) {
    return (
      <div className="space-y-3">
        <Button variant="ghost" size="sm" onClick={() => navigate('/items')}>
          <ArrowLeftIcon className="size-4" /> Back to items
        </Button>
        <p className="text-destructive text-sm">Could not load item.</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      <DetailHeader
        item={item}
        onEdit={() => setEditOpen(true)}
        onDelete={() => setDeleteOpen(true)}
      />

      <div className="grid gap-3 lg:grid-cols-2">
        <SummaryCard item={item} />
        <FilesCard item={item} onUpload={() => setUploadOpen(true)} />
        <ActivityCard item={item} />
      </div>

      <ItemFormDialog open={editOpen} onOpenChange={setEditOpen} item={item} />
      <DeleteItemDialog item={item} open={deleteOpen} onOpenChange={setDeleteOpen} />
      <UploadFilesDialog item={item} open={uploadOpen} onOpenChange={setUploadOpen} />
    </div>
  )
}

function DetailHeader({
  item,
  onEdit,
  onDelete,
}: {
  item: Item
  onEdit: () => void
  onDelete: () => void
}) {
  const canWrite = useCanWrite()
  return (
    <header className="space-y-2">
      <nav className="text-foreground flex items-center gap-1 text-xs">
        <Link to="/items" className="hover:text-foreground hover:underline">
          Items
        </Link>
        <span aria-hidden>›</span>
        <span className="text-foreground">{item.name}</span>
      </nav>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-baseline gap-3">
          <h1 className="text-2xl font-semibold tracking-tight">{item.name}</h1>
          <Badge variant="secondary" className={item.isActive ? 'bg-green-600' : 'bg-gray-600'}>
            {item.isActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
        {canWrite && (
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={onEdit}>
              <PencilIcon className="size-4" />
              Edit
            </Button>
            <Button variant="outline" onClick={onDelete}>
              <Trash2Icon className="size-4" />
              Delete
            </Button>
          </div>
        )}
      </div>
    </header>
  )
}

function SummaryCard({ item }: { item: Item }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Summary</CardTitle>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-x-6 gap-y-4 text-sm">
        <Field label="Name" value={item.name} />
        <Field label="Owner" value={item.owner ?? '—'} />
        <Field label="Status" value={item.isActive ? 'Active' : 'Inactive'} />
        <Field label="Created" value={formatDate(item.createdOn)} />
        <div className="col-span-2">
          <Field label="Description" value={item.description ?? '—'} />
        </div>
      </CardContent>
    </Card>
  )
}

function Field({
  label,
  value,
  valueClassName,
}: {
  label: string
  value: string
  valueClassName?: string
}) {
  return (
    <div className="space-y-1">
      <p className="text-accent text-[10px] font-semibold tracking-wider uppercase">{label}</p>
      <p className={cn('text-muted-foreground', valueClassName)}>{value}</p>
    </div>
  )
}

function FilesCard({ item, onUpload }: { item: Item; onUpload: () => void }) {
  const files = item.files ?? []
  const canWrite = useCanWrite()
  const downloadFile = useDownloadItemFile()
  const pendingId = downloadFile.isPending ? downloadFile.variables?.fileId : undefined

  const onDownload = (fileId: string, fileName: string) => {
    downloadFile.mutate(
      { entityId: item.id, fileId, fileName },
      {
        onError: (error) => toast.error(extractErrorMessage(error, 'Could not start download')),
      },
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Files</CardTitle>
        {canWrite && (
          <Button size="sm" onClick={onUpload}>
            <PlusIcon className="size-4" />
            Upload file
          </Button>
        )}
      </CardHeader>
      <CardContent>
        {files.length === 0 ? (
          <p className="text-muted-foreground text-sm">No files attached.</p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>File</TableHead>
                <TableHead>Uploaded</TableHead>
                <TableHead>By</TableHead>
                <TableHead className="w-2" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {files.map((file) => {
                const isPending = pendingId === file.id
                return (
                  <TableRow key={file.id}>
                    <TableCell className="text-primary font-medium">{file.name}</TableCell>
                    <TableCell className="text-muted-foreground tabular-nums">
                      {formatDate(file.uploadedOn)}
                    </TableCell>
                    <TableCell className="text-muted-foreground">{file.uploadedBy}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="link"
                        size="sm"
                        onClick={() => onDownload(file.id, file.name)}
                        disabled={isPending}
                      >
                        <DownloadIcon className="size-4" />
                        {isPending ? 'Preparing…' : 'Download'}
                      </Button>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  )
}

function ActivityCard({ item }: { item: Item }) {
  const items = [
    item.updatedOn && item.updatedOn !== item.createdOn
      ? { label: 'Updated', who: item.updatedBy, when: new Date(item.updatedOn), dot: 'bg-primary' }
      : null,
    { label: 'Created', who: item.createdBy, when: new Date(item.createdOn), dot: 'bg-green-500' },
  ].filter(Boolean) as { label: string; who: string; when: Date; dot: string }[]

  return (
    <Card>
      <CardHeader>
        <CardTitle>Activity</CardTitle>
      </CardHeader>
      <CardContent>
        <ol className="space-y-4">
          {items.map((entry, idx) => (
            <li key={idx} className="relative pl-5">
              <span className={cn('absolute top-1.5 left-0 size-2.5 rounded-full', entry.dot)} />
              {idx < items.length - 1 && (
                <span className="bg-border absolute top-4 left-[5px] h-full w-px" />
              )}
              <p className="text-sm font-semibold">
                {entry.label}{' '}
                <span className="text-muted-foreground font-normal">by {entry.who}</span>
              </p>
              <p className="text-muted-foreground text-xs tabular-nums">
                {entry.when.toLocaleString()}
              </p>
            </li>
          ))}
        </ol>
      </CardContent>
    </Card>
  )
}

function DeleteItemDialog({
  item,
  open,
  onOpenChange,
}: {
  item: Item
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const navigate = useNavigate()
  const deleteItem = useDeleteItem()

  const onConfirm = () => {
    deleteItem.mutate(item.id, {
      onSuccess: () => {
        toast.success(`Deleted ${item.name}`)
        onOpenChange(false)
        navigate('/items')
      },
      onError: (error) => toast.error(extractErrorMessage(error, 'Could not delete item')),
    })
  }

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {item.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            This soft-deletes the item; it is hidden from listings but retained in the audit trail.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={deleteItem.isPending}>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={deleteItem.isPending}>
            {deleteItem.isPending ? 'Deleting…' : 'Delete'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

function UploadFilesDialog({
  item,
  open,
  onOpenChange,
}: {
  item: Item
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const [files, setFiles] = useState<File[]>([])
  const [isDragging, setIsDragging] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const upload = useUploadItemFiles()

  // Picked-but-unsent files belong to one opening of the dialog. Clearing them as it closes (rather
  // than syncing state in an effect when it opens) means the next opening starts empty without a
  // render-then-setState cascade.
  const handleOpenChange = (next: boolean) => {
    if (!next) setFiles([])
    onOpenChange(next)
  }

  const addFiles = (list: FileList | null) => {
    if (!list || list.length === 0) return
    setFiles((prev) => [...prev, ...Array.from(list)])
  }
  const removeAt = (index: number) => setFiles((prev) => prev.filter((_, i) => i !== index))

  const onSubmit = (event: React.FormEvent) => {
    event.preventDefault()
    if (files.length === 0 || upload.isPending) return
    upload.mutate(
      { itemId: item.id, files },
      {
        onSuccess: () => {
          toast.success(`Uploaded ${files.length} file(s)`)
          handleOpenChange(false)
        },
        onError: (error) => toast.error(extractErrorMessage(error, 'Upload failed')),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Upload files</DialogTitle>
          <DialogDescription>Attach documents or images to {item.name}.</DialogDescription>
        </DialogHeader>
        <form onSubmit={onSubmit} className="space-y-3">
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            onDragOver={(event) => {
              event.preventDefault()
              setIsDragging(true)
            }}
            onDragLeave={() => setIsDragging(false)}
            onDrop={(event) => {
              event.preventDefault()
              setIsDragging(false)
              addFiles(event.dataTransfer.files)
            }}
            className={cn(
              'flex w-full flex-col items-center gap-2 rounded-md border border-dashed p-6 transition-colors',
              isDragging
                ? 'border-primary bg-primary/5'
                : 'border-input bg-muted/30 hover:bg-muted/50',
            )}
          >
            <span className="bg-primary/10 text-primary flex size-10 items-center justify-center rounded-full">
              <UploadCloudIcon className="size-5" />
            </span>
            <span className="text-sm font-semibold">Drop files here or click to browse</span>
            <span className="text-muted-foreground text-xs">PDF, JPG, PNG · up to 10 MB each</span>
            <input
              ref={inputRef}
              type="file"
              multiple
              className="hidden"
              onChange={(event) => {
                addFiles(event.target.files)
                event.target.value = ''
              }}
            />
          </button>

          {files.length > 0 && (
            <ul className="max-h-48 space-y-1 overflow-y-auto">
              {files.map((file, index) => (
                <li
                  key={`${index}-${file.name}`}
                  className="bg-muted/30 flex items-center gap-2 rounded-md border px-2 py-1.5 text-sm"
                >
                  <FileTextIcon className="text-primary size-4 shrink-0" />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">{file.name}</p>
                    <p className="text-muted-foreground text-[10px]">{formatBytes(file.size)}</p>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="size-7 p-0"
                    onClick={() => removeAt(index)}
                    disabled={upload.isPending}
                    aria-label={`Remove ${file.name}`}
                  >
                    <XIcon className="size-4" />
                  </Button>
                </li>
              ))}
            </ul>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={upload.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={files.length === 0 || upload.isPending}>
              {upload.isPending ? 'Uploading…' : `Upload ${files.length || ''}`}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function formatBytes(bytes: number | null | undefined): string {
  if (bytes === null || bytes === undefined || Number.isNaN(bytes)) return '—'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`
}

function extractErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiError && error.body && typeof error.body === 'object') {
    const body = error.body as { detail?: string; title?: string }
    return body.detail ?? body.title ?? fallback
  }
  if (error instanceof Error) return error.message
  return fallback
}

function ItemDetailSkeleton() {
  return (
    <div className="space-y-3">
      <Skeleton className="h-4 w-32" />
      <Skeleton className="h-8 w-64" />
      <div className="grid gap-3 lg:grid-cols-2">
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i}>
            <CardContent className="space-y-2 pt-6">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
