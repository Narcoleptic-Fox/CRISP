import { useState } from 'react'
import { NavLink } from 'react-router-dom'
import { MenuIcon } from 'lucide-react'
import { cn } from '@/lib/utils'
import logoDark from '@/assets/logo_white.svg'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'

// No authentication: no signed-in user to show, no role to filter by, no sign-out. This file
// replaces top-nav.tsx when the solution is generated with --auth none.
const navItems = [
  { to: '/', label: 'Dashboard', end: true },
  //#if (IncludeSample)
  { to: '/items', label: 'Items', end: false },
  //#endif
] as const

export function TopNav() {
  const [open, setOpen] = useState(false)

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
              <SheetTitle className="text-muted-foreground truncate">CRISP</SheetTitle>
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
      </div>
    </header>
  )
}
