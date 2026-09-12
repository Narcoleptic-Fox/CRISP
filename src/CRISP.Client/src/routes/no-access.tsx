import { useMsal } from '@azure/msal-react'
import { Button } from '@/components/ui/button'
import { useCurrentUser } from '@/auth/useCurrentUser'

export function NoAccessPage() {
  const { instance } = useMsal()
  const { data } = useCurrentUser()
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0]
  const deactivated = data?.status === 'deactivated'

  return (
    <main className="bg-background text-foreground flex min-h-svh flex-col items-center justify-center px-6">
      <div className="w-full max-w-md space-y-6 text-center">
        <p className="text-muted-foreground text-xs font-medium tracking-wider uppercase">
          Asset Calibration Manager
        </p>
        <h1 className="text-foreground text-3xl font-semibold tracking-tight">
          {deactivated ? 'Account deactivated' : 'Access not provisioned'}
        </h1>
        <p className="text-foreground text-sm leading-6">
          {deactivated
            ? 'Your account exists but is currently inactive. Contact an administrator to restore access.'
            : 'You signed in successfully, but no account is set up for your email. Contact an administrator to request access.'}
        </p>
        {account?.username && (
          <p className="text-muted-foreground text-xs">
            Signed in as <span className="font-medium">{account.username}</span>
          </p>
        )}
        <div className="flex justify-center gap-2 pt-2">
          <Button type="button" variant="outline" onClick={() => instance.logoutRedirect()}>
            Sign out
          </Button>
        </div>
      </div>
    </main>
  )
}
