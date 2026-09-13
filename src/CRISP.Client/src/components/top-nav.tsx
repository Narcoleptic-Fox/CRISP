import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useMsal } from '@azure/msal-react'
import { MenuIcon } from 'lucide-react'
import { cn } from '@/lib/utils'
import logoDark from '@/assets/logo_white.svg'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { Roles } from '@/api/roles'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'
import { UserMenu } from './user-menu'

const baseNavItems = [
  { to: '/', label: 'Dashboard', end: true, adminOnly: false },
  //#if (IncludeSample)
  { to: '/items', label: 'Items', end: false, adminOnly: false },
  //#endif
  { to: '/users', label: 'Users', end: false, adminOnly: true },
] as const

function initialsFrom(name: string | undefined): string {
  if (!name) return '?'
  const parts = name.split(/[\s,@.]+/).filter(Boolean)
  if (parts.length === 0) return '?'
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return (parts[0][0] + parts[1][0]).toUpperCase()
}

export function TopNav() {
  const { data } = useCurrentUser()
  const { instance } = useMsal()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)

  const isAdmin = data?.user?.role === Roles.Admin
  const navItems = baseNavItems.filter((item) => !item.adminOnly || isAdmin)

  const user = data?.user
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0]
  const displayName = user?.username ?? account?.name ?? account?.username ?? ''
  const email = user?.email ?? account?.username ?? ''

  return (
    <header className="bg-appbar text-appbar-foreground dark:bg-appbar dark:text-appbar-foreground border-border sticky top-0 z-30 border-b dark:border-transparent">
      <div className="relative mx-auto flex h-15 w-full items-center px-4 md:px-6">
        <Sheet open={open} onOpenChange={setOpen}>
          <SheetTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Open menu"
              className="text-appbar-foreground hover:bg-primary mr-2 md:hidden"
            >
              <MenuIcon />
            </Button>
          </SheetTrigger>
          <SheetContent side="left" className="w-72 p-0">
            <SheetHeader className="border-border border-b">
              <div className="flex items-center gap-3">
                <Avatar className="size-9">
                  <AvatarFallback className="bg-primary text-primary-foreground text-xs font-semibold">
                    {initialsFrom(displayName)}
                  </AvatarFallback>
                </Avatar>
                <div className="flex min-w-0 flex-col">
                  <SheetTitle className="text-muted-foreground truncate">
                    {displayName || 'Signed in'}
                  </SheetTitle>
                  {email && (
                    <SheetDescription className="truncate text-xs">{email}</SheetDescription>
                  )}
                </div>
              </div>
            </SheetHeader>

            <nav className="flex flex-1 flex-col gap-1 p-2">
              {navItems.map((item) => (
                <SheetClose asChild key={item.to}>
                  <NavLink
                    to={item.to}
                    end={item.end}
                    className={({ isActive }) =>
                      cn(
                        'text-primary rounded-md px-3 py-2 transition-colors',
                        isActive
                          ? 'text-primary-foreground font-semibold'
                          : 'text-muted-foreground hover:bg-muted',
                      )
                    }
                  >
                    {item.label}
                  </NavLink>
                </SheetClose>
              ))}
            </nav>

            <div className="border-border flex flex-col gap-1 border-t p-2">
              <Button
                variant="ghost"
                className="justify-start"
                onClick={() => {
                  setOpen(false)
                  navigate('/settings')
                }}
              >
                Settings
              </Button>
              <Button
                variant="ghost"
                className="text-destructive hover:text-destructive justify-start"
                onClick={() => instance.logoutRedirect()}
              >
                Sign out
              </Button>
            </div>
          </SheetContent>
        </Sheet>

        <NavLink to="/" aria-label="CRISP home" className="flex shrink-0 items-center">
          <img src={logoDark} alt="CRISP" className="h-8 w-auto dark:hidden" />
          <img src={logoDark} alt="CRISP" className="hidden h-8 w-auto dark:block" />
        </NavLink>

        <nav className="absolute left-1/2 hidden -translate-x-1/2 items-center gap-1 md:flex">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                cn(
                  'text-md relative px-3 py-4 transition-colors',
                  isActive
                    ? 'text-appbar-foreground bg-primary font-semibold'
                    : 'text-appbar-foreground hover:bg-primary',
                )
              }
            >
              {({ isActive }) => (
                <>
                  {item.label}
                  {isActive && (
                    <span className="bg-primary absolute right-3 bottom-0 left-3 h-0.5 rounded-full" />
                  )}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="ml-auto hidden items-center gap-3 md:flex">
          <UserMenu />
        </div>
      </div>
    </header>
  )
}
