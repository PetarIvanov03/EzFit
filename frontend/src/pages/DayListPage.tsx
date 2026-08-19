import { useDayList } from "@/api/days"
import { DaySummaryCard } from "@/components/days/DaySummaryCard"
import { Skeleton } from "@/components/ui/skeleton"

export function DayListPage() {
  const { data, isPending, isError, error } = useDayList(7)

  return (
    <div className="flex flex-col gap-4">
      <h1 className="text-2xl font-semibold">Days</h1>

      {isPending && (
        <div className="flex flex-col gap-3">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">
          Failed to load days: {error instanceof Error ? error.message : "Unknown error"}
        </p>
      )}

      {data && data.length === 0 && (
        <p className="text-sm text-muted-foreground">No days yet.</p>
      )}

      {data && data.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.map((day) => (
            <DaySummaryCard key={day.date} day={day} />
          ))}
        </div>
      )}
    </div>
  )
}
