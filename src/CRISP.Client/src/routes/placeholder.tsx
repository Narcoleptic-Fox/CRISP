interface PlaceholderPageProps {
  title: string
}

export function PlaceholderPage({ title }: PlaceholderPageProps) {
  return (
    <div className="space-y-2">
      <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
      <p className="text-foreground text-sm">This screen isn&apos;t built yet. Coming soon.</p>
    </div>
  )
}
