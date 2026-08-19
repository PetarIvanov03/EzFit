import { Link, useParams } from "react-router-dom"
import { useDay } from "@/api/days"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { EntryCard } from "@/components/entries/EntryCard"
import { entryTypeStyles, entryTypeOrder } from "@/components/entries/entry-type-styles"
import { formatDateHeading, formatMinutes } from "@/lib/format"

export function DayDetailPage() {
  const { date } = useParams<{ date: string }>()
  const { data: day, isPending, isError, error } = useDay(date ?? "")

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">
          {date ? formatDateHeading(date) : "Day"}
        </h1>
        <Button asChild variant="outline">
          <Link to="/">Back</Link>
        </Button>
      </div>

      {isPending && (
        <div className="flex flex-col gap-3">
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">
          Failed to load day: {error instanceof Error ? error.message : "Unknown error"}
        </p>
      )}

      {day && (
        <>
          <div className="grid grid-cols-3 gap-4 rounded-md border border-border bg-card p-4 text-sm">
            <div>
              <div className="text-muted-foreground">Calories</div>
              <div className="text-base font-medium">
                {day.totalKcalIn} in · {day.totalKcalOut} out
              </div>
            </div>
            <div>
              <div className="text-muted-foreground">Macros</div>
              <div className="text-base font-medium">
                {day.totalProtein}P · {day.totalFats}F · {day.totalCarbs}C
              </div>
            </div>
            <div>
              <div className="text-muted-foreground">Sleep</div>
              <div className="text-base font-medium">
                {formatMinutes(day.totalSleepMin)}
                {day.sleepScore !== null ? ` · Score ${day.sleepScore}` : ""}
              </div>
            </div>
          </div>

          {day.entries.length === 0 && (
            <p className="text-sm text-muted-foreground">No entries yet.</p>
          )}

          {entryTypeOrder.map((type) => {
            const entries = day.entries.filter((entry) => entry.type === type)
            if (entries.length === 0) return null

            return (
              <section key={type} className="flex flex-col gap-3">
                <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                  {entryTypeStyles[type].label} ({entries.length})
                </h2>
                <div className="flex flex-col gap-2">
                  {entries.map((entry) => (
                    <EntryCard key={entry.id} entry={entry} />
                  ))}
                </div>
              </section>
            )
          })}
        </>
      )}
    </div>
  )
}
