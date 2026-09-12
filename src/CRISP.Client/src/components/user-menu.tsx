import { useMsal } from '@azure/msal-react'
import { useNavigate } from 'react-router-dom'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useCurrentUser } from '@/auth/useCurrentUser'

function initialsFrom(name: string | undefined): string {
  if (!name) return '?'
  const parts = name.split(/[\s,@.]+/).filter(Boolean)
  if (parts.length === 0) return '?'
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return (parts[0][0] + parts[1][0]).toUpperCase()
}

export function UserMenu() {
  const { instance } = useMsal()
  const { data } = useCurrentUser()
  const navigate = useNavigate()
  const user = data?.user
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0]
  const displayName = user?.username ?? account?.name ?? account?.username ?? ''
  const email = user?.email ?? account?.username ?? ''

  return (
    <DropdownMenu>
      <DropdownMenuTrigger className="focus-visible:outline-ring rounded-full focus-visible:outline-2 focus-visible:outline-offset-2">
        <Avatar className="size-8">
          <AvatarFallback className="bg-primary text-primary-foreground text-xs font-semibold">
            {initialsFrom(displayName)}
          </AvatarFallback>
        </Avatar>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>
          <div className="flex flex-col">
            <span className="font-medium">{displayName || 'Signed in'}</span>
            {email && <span className="text-muted-foreground text-xs font-normal">{email}</span>}
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem onSelect={() => navigate('/settings')}>Settings</DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onSelect={() => instance.logoutRedirect()}
          className="text-destructive focus:text-destructive"
        >
          Sign out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
