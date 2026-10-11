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
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { ApiError } from '@/lib/api'
import { Roles } from '@/api/roles'
import type { User } from '@/api/user'
import { useCreateUser, useUpdateUser } from './useUserMutations'

const formSchema = z.object({
  firstName: z.string().trim().min(1, 'First name is required').max(50),
  lastName: z.string().trim().min(1, 'Last name is required').max(50),
  email: z.string().trim().min(1, 'Email is required').email('Invalid email'),
  role: z.nativeEnum(Roles),
  isActive: z.boolean(),
})

type FormValues = z.infer<typeof formSchema>

interface UserFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  user?: User | null
}

const ROLE_OPTIONS = [
  { value: Roles.Viewer, label: 'Viewer' },
  { value: Roles.Editor, label: 'Editor' },
  { value: Roles.Admin, label: 'Admin' },
] as const

// Username canonical format is "Lastname, Firstname" so it sorts and displays
// consistently across the app.
function formatUsername(firstName: string, lastName: string): string {
  return `${lastName.trim()}, ${firstName.trim()}`
}

function splitUsername(username: string): { firstName: string; lastName: string } {
  const separator = username.indexOf(', ')
  if (separator === -1) {
    return { firstName: '', lastName: username }
  }
  return {
    lastName: username.slice(0, separator),
    firstName: username.slice(separator + 2),
  }
}

export function UserFormDialog({ open, onOpenChange, user }: UserFormDialogProps) {
  const isEdit = !!user
  const createUser = useCreateUser()
  const updateUser = useUpdateUser()

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      role: Roles.Viewer,
      isActive: true,
    },
  })

  useEffect(() => {
    if (open) {
      if (user) {
        const { firstName, lastName } = splitUsername(user.username)
        form.reset({
          firstName,
          lastName,
          email: user.email,
          role: user.role,
          isActive: user.isActive,
        })
      } else {
        form.reset({
          firstName: '',
          lastName: '',
          email: '',
          role: Roles.Viewer,
          isActive: true,
        })
      }
    }
  }, [open, user, form])

  const isSubmitting = createUser.isPending || updateUser.isPending

  const onSubmit = (values: FormValues) => {
    const username = formatUsername(values.firstName, values.lastName)
    const payload = {
      username,
      email: values.email,
      role: values.role,
      isActive: values.isActive,
    }

    if (isEdit && user) {
      updateUser.mutate(
        { id: user.id, ...payload },
        {
          onSuccess: () => {
            toast.success(`Updated ${username}`)
            onOpenChange(false)
          },
          onError: (error) => {
            toast.error(extractErrorMessage(error, 'Could not update user'))
          },
        },
      )
    } else {
      createUser.mutate(payload, {
        onSuccess: () => {
          toast.success(`Created ${username}`)
          onOpenChange(false)
        },
        onError: (error) => {
          toast.error(extractErrorMessage(error, 'Could not create user'))
        },
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit user' : 'New user'}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Update the user’s profile, role, or active status.'
              : 'Provision a new user. They must sign in with Entra to activate.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <FormField
                control={form.control}
                name="firstName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>First name</FormLabel>
                    <FormControl>
                      <Input autoComplete="off" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="lastName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Last name</FormLabel>
                    <FormControl>
                      <Input autoComplete="off" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" autoComplete="off" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="role"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Role</FormLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(value) => field.onChange(Number(value) as Roles)}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent position="popper">
                      {ROLE_OPTIONS.map((option) => (
                        <SelectItem key={option.value} value={String(option.value)}>
                          {option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="isActive"
              render={({ field }) => (
                <FormItem>
                  <div className="bg-muted/20 flex items-center justify-between rounded-md border p-3">
                    <div>
                      <Label className="text-sm font-medium">Active</Label>
                      <p className="text-muted-foreground text-xs">
                        Inactive users cannot sign in or be assigned work.
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
                {isEdit ? 'Save changes' : 'Create user'}
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
