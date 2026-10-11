import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Textarea } from '@/components/ui/textarea'
import { ApiError } from '@/lib/api'
import type { Item } from '@/api/item'
import { useCreateItem, useUpdateItem } from './useItemMutations'
import { useItemOwnerNames } from './useItemOwnerNames'

const formSchema = z.object({
  name: z.string().trim().min(1, 'Name is required').max(100),
  description: z.string().trim().max(1000).optional(),
  owner: z.string().trim().max(100).optional(),
  isActive: z.boolean(),
})

type FormValues = z.infer<typeof formSchema>

const OWNER_DATALIST_ID = 'item-owner-options'

interface ItemFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  item?: Item | null
}

export function ItemFormDialog({ open, onOpenChange, item }: ItemFormDialogProps) {
  const isEdit = !!item
  const createItem = useCreateItem()
  const updateItem = useUpdateItem()
  const { data: ownerNames } = useItemOwnerNames()

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: { name: '', description: '', owner: '', isActive: true },
  })

  useEffect(() => {
    if (open) {
      form.reset(
        item
          ? {
              name: item.name,
              description: item.description ?? '',
              owner: item.owner ?? '',
              isActive: item.isActive,
            }
          : { name: '', description: '', owner: '', isActive: true },
      )
    }
  }, [open, item, form])

  const isSubmitting = createItem.isPending || updateItem.isPending

  const onSubmit = (values: FormValues) => {
    const description = values.description?.trim() ? values.description.trim() : null
    const owner = values.owner?.trim() ? values.owner.trim() : null

    if (isEdit && item) {
      updateItem.mutate(
        { id: item.id, name: values.name, description, owner, isActive: values.isActive },
        {
          onSuccess: () => {
            toast.success(`Updated ${values.name}`)
            onOpenChange(false)
          },
          onError: (error) => toast.error(extractErrorMessage(error, 'Could not update item')),
        },
      )
    } else {
      createItem.mutate(
        { name: values.name, description, owner },
        {
          onSuccess: () => {
            toast.success(`Created ${values.name}`)
            onOpenChange(false)
          },
          onError: (error) => toast.error(extractErrorMessage(error, 'Could not create item')),
        },
      )
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit item' : 'New item'}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Update the item details or deactivate it.'
              : 'Add a new item to the catalog.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input autoComplete="off" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Textarea rows={3} {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="owner"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Owner</FormLabel>
                  <FormControl>
                    <Input autoComplete="off" list={OWNER_DATALIST_ID} {...field} />
                  </FormControl>
                  <datalist id={OWNER_DATALIST_ID}>
                    {(ownerNames ?? []).map((option) => (
                      <option key={option.name} value={option.name} />
                    ))}
                  </datalist>
                  <FormDescription>
                    Free text. Suggestions are drawn from owners already in use.
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {isEdit && (
              <FormField
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <FormItem>
                    <div className="bg-muted/20 flex items-center justify-between rounded-md border p-3">
                      <div>
                        <Label className="text-sm font-medium">Active</Label>
                        <p className="text-muted-foreground text-xs">
                          Inactive items are hidden from the default active filter.
                        </p>
                      </div>
                      <FormControl>
                        <Switch checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                    </div>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={isSubmitting}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isEdit ? 'Save changes' : 'Create item'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}

function extractErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiError && error.body && typeof error.body === 'object') {
    const body = error.body as { detail?: string; title?: string }
    return body.detail ?? body.title ?? fallback
  }
  return fallback
}
