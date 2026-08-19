import { Badge } from "@/components/ui/badge"
import { entryTypeStyles } from "@/components/entries/entry-type-styles"
import { formatEntry } from "@/components/entries/entry-format"
import { formatTime } from "@/lib/format"
import type { EntryDto } from "@/types/entry"

export function EntryCard({ entry }: { entry: EntryDto }) {
  const style = entryTypeStyles[entry.type]
  const { detail, metrics } = formatEntry(entry)

  return (
    <div
      className={`rounded-md border border-l-4 bg-card p-4 shadow-sm ${style.border}`}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <span className="font-medium text-card-foreground">
              {entry.title ?? style.label}
            </span>
            <Badge variant="secondary" className={style.badge}>
              {style.label}
            </Badge>
          </div>
          {detail && <p className="mt-1 text-sm text-muted-foreground">{detail}</p>}
        </div>
        <div className="shrink-0 text-right text-sm text-muted-foreground">
          <div>{formatTime(entry.occurredAt ?? entry.createdAt)}</div>
          {metrics.map((metric) => (
            <div key={metric}>{metric}</div>
          ))}
        </div>
      </div>
    </div>
  )
}
