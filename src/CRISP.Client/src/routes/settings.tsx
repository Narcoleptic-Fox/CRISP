import { useState } from 'react'
import { useMsal } from '@azure/msal-react'
import { useTheme } from 'next-themes'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { useUpdateUserSettings } from '@/features/settings/useUpdateUserSettings'
import { Roles } from '@/api/roles'
import { cn } from '@/lib/utils'

const THEME_OPTIONS = [
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
  { value: 'system', label: 'System' },
] as const

function roleLabel(role: Roles | number): string {
  switch (role) {
    case Roles.Admin:
      return 'Admin'
    case Roles.Editor:
      return 'Editor'
    case Roles.Viewer:
      return 'Viewer'
    default:
      return 'Unknown'
  }
}

export function SettingsPage() {
  const { instance } = useMsal()
  const { data, isLoading } = useCurrentUser()
  const updateSettings = useUpdateUserSettings()
  const { theme, setTheme, resolvedTheme } = useTheme()
  const user = data?.user

  const savedNotificationsOn = user?.settings?.allowNotifications ?? true
  const [optimisticNotifications, setOptimisticNotifications] = useState<{
    userId: string
    value: boolean
  } | null>(null)
  const notificationsOn =
    optimisticNotifications && optimisticNotifications.userId === user?.id
      ? optimisticNotifications.value
      : savedNotificationsOn

  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0]

  if (isLoading || !user) {
    return <p className="text-muted-foreground text-sm">Loading...</p>
  }

  const handleNotificationsChange = (next: boolean) => {
    setOptimisticNotifications({ userId: user.id, value: next })
    updateSettings.mutate(
      { id: user.id, allowNotifications: next },
      {
        onSuccess: () => toast.success('Notification preferences updated'),
        onError: () => {
          setOptimisticNotifications(null)
          toast.error('Could not update preferences')
        },
      },
    )
  }

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-sm">Manage your profile and notification preferences.</p>
      </header>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Profile</CardTitle>
            <p className="text-muted-foreground text-xs">
              Read-only. Contact an admin to change these values.
            </p>
          </CardHeader>
          <CardContent className="space-y-4">
            <Field label="Username" value={user.username} />
            <Field label="Email" value={user.email} />
            <div className="space-y-1">
              <p className="text-accent text-[10px] font-semibold tracking-wider uppercase">Role</p>
              <Badge variant="secondary" className="font-semibold">
                {roleLabel(user.role)}
              </Badge>
            </div>
            <div className="space-y-1">
              <p className="text-accent text-[10px] font-semibold tracking-wider uppercase">
                Account
              </p>
              <Badge
                variant="secondary"
                className={
                  user.isActive
                    ? 'bg-green-100 text-green-800 hover:bg-green-100'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-100'
                }
              >
                {user.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Notification preferences</CardTitle>
            <p className="text-muted-foreground text-xs">
              Control which email reminders this account receives.
            </p>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="bg-muted/20 flex items-start justify-between gap-4 rounded-md border p-4">
              <div className="space-y-1">
                <Label htmlFor="reminders-toggle" className="text-sm font-semibold">
                  Calibration reminders
                </Label>
                <p className="text-muted-foreground text-xs leading-5">
                  Email you when assets you own become due in 30 days, due today, or overdue.
                </p>
              </div>
              <Switch
                id="reminders-toggle"
                checked={notificationsOn}
                disabled={updateSettings.isPending}
                onCheckedChange={handleNotificationsChange}
              />
            </div>
            <p className="text-muted-foreground text-xs">
              Turning reminders off does not affect dashboard visibility.
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Session</CardTitle>
            <p className="text-muted-foreground text-xs">Your current authenticated session.</p>
          </CardHeader>
          <CardContent className="space-y-4">
            <Field label="Auth method" value="Microsoft Entra ID (JWT bearer)" />
            <Field label="Signed in as" value={account?.username ?? '—'} />
            <Field label="User id" value={user.id} mono />
            <Button
              variant="outline"
              className="text-destructive hover:text-destructive"
              onClick={() => instance.logoutRedirect()}
            >
              Sign out
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Appearance</CardTitle>
            <p className="text-muted-foreground text-xs">Choose how CRISP looks on this device.</p>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="bg-muted/40 inline-flex items-center rounded-md border p-1">
              {THEME_OPTIONS.map((option) => {
                const isActive = (theme ?? 'system') === option.value
                return (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => setTheme(option.value)}
                    className={cn(
                      'rounded-sm px-4 py-1.5 text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-background text-foreground shadow-sm'
                        : 'text-muted-foreground hover:text-foreground',
                    )}
                    aria-pressed={isActive}
                  >
                    {option.label}
                  </button>
                )
              })}
            </div>
            <p className="text-muted-foreground text-xs">
              {theme === 'system'
                ? `Following your OS preference (currently ${resolvedTheme ?? 'light'}).`
                : `Set to ${theme ?? 'system'} on this device.`}
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

function Field({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="space-y-1">
      <p className="text-accent text-[10px] font-semibold tracking-wider uppercase">{label}</p>
      <p className={mono ? 'font-mono text-xs break-all' : 'text-sm'}>{value}</p>
    </div>
  )
}
