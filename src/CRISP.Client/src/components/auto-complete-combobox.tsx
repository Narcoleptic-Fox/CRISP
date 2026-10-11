import { useState } from 'react'
import { CheckIcon, ChevronsUpDownIcon, XIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { cn } from '@/lib/utils'
import type { AutoComplete } from '@/api/auto-complete'

interface AutoCompleteComboboxProps {
  options: AutoComplete[] | undefined
  value: string | null
  onChange: (id: string | null) => void
  placeholder?: string
  emptyText?: string
  loading?: boolean
  disabled?: boolean
  allowClear?: boolean
  className?: string
}

export function AutoCompleteCombobox({
  options,
  value,
  onChange,
  placeholder = 'Select…',
  emptyText = 'No matches.',
  loading = false,
  disabled = false,
  allowClear = true,
  className,
}: AutoCompleteComboboxProps) {
  const [open, setOpen] = useState(false)
  const selected = options?.find((o) => o.id === value) ?? null

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          type="button"
          variant="outline"
          role="combobox"
          aria-expanded={open}
          disabled={disabled || loading}
          className={cn(
            'w-full justify-between font-normal',
            !selected && 'text-muted-foreground',
            className,
          )}
        >
          <span className="truncate">{loading ? 'Loading…' : (selected?.name ?? placeholder)}</span>
          <span className="ml-2 flex shrink-0 items-center gap-1">
            {allowClear && selected && !disabled && (
              <XIcon
                className="hover:text-foreground text-muted-foreground size-4"
                onClick={(event) => {
                  event.preventDefault()
                  event.stopPropagation()
                  onChange(null)
                }}
              />
            )}
            <ChevronsUpDownIcon className="text-muted-foreground size-4" />
          </span>
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search…" />
          <CommandList>
            <CommandEmpty>{emptyText}</CommandEmpty>
            <CommandGroup>
              {(options ?? []).map((option) => (
                <CommandItem
                  key={option.id}
                  value={option.name}
                  onSelect={() => {
                    onChange(option.id)
                    setOpen(false)
                  }}
                >
                  <CheckIcon
                    className={cn('size-4', option.id === value ? 'opacity-100' : 'opacity-0')}
                  />
                  {option.name}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
