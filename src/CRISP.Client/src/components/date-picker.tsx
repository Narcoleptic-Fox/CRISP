import { useState } from 'react'
import { CalendarIcon, XIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Calendar } from '@/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { cn } from '@/lib/utils'

interface DatePickerProps {
  value: Date | null
  onChange: (date: Date | null) => void
  placeholder?: string
  disabled?: boolean
  allowClear?: boolean
  fromDate?: Date
  toDate?: Date
  className?: string
}

function formatDate(date: Date): string {
  return date.toLocaleDateString('en-CA')
}

export function DatePicker({
  value,
  onChange,
  placeholder = 'Pick a date',
  disabled = false,
  allowClear = true,
  fromDate,
  toDate,
  className,
}: DatePickerProps) {
  const [open, setOpen] = useState(false)

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          type="button"
          variant="outline"
          disabled={disabled}
          className={cn(
            'w-full justify-start gap-2 font-normal',
            !value && 'text-muted-foreground',
            className,
          )}
        >
          <CalendarIcon className="size-4" />
          <span className="flex-1 truncate text-left">
            {value ? formatDate(value) : placeholder}
          </span>
          {allowClear && value && !disabled && (
            <XIcon
              className="hover:text-foreground text-muted-foreground size-4"
              onClick={(event) => {
                event.preventDefault()
                event.stopPropagation()
                onChange(null)
              }}
            />
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="single"
          selected={value ?? undefined}
          onSelect={(date) => {
            onChange(date ?? null)
            setOpen(false)
          }}
          disabled={(date) => {
            if (fromDate && date < fromDate) return true
            if (toDate && date > toDate) return true
            return false
          }}
          autoFocus
        />
      </PopoverContent>
    </Popover>
  )
}
