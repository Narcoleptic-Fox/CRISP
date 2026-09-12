import logoLight from '@/assets/logo.svg'
import logoDark from '@/assets/logo_white.svg'

interface FullPageLoaderProps {
  message?: string
}

export function FullPageLoader({ message }: FullPageLoaderProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      aria-busy="true"
      className="bg-background fixed inset-0 flex flex-col items-center justify-center gap-6 dark:bg-transparent"
    >
      <img src={logoLight} alt="CRISP" className="h-16 w-auto dark:hidden" />
      <img src={logoDark} alt="CRISP" className="hidden h-16 w-auto dark:block" />
      <div
        aria-hidden="true"
        className="border-primary/20 border-t-primary h-8 w-8 animate-spin rounded-full border-[3px] dark:border-white/20 dark:border-t-white"
      />
      {message && <p className="text-muted-foreground text-sm">{message}</p>}
    </div>
  )
}
