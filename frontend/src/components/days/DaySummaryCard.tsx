import { Link } from "react-router-dom"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { formatMinutes } from "@/lib/format"
import { EntryType, type DaySummaryDto } from "@/types/entry"

export function DaySummaryCard({ day }: { day: DaySummaryDto }) {
  const workoutsCount = day.entries.filter(
    (entry) => entry.type === EntryType.Activity,
  ).length

  return (
    <Link to={`/days/${day.date}`}>
      <Card className="transition-colors hover:border-primary/50">
        <CardHeader>
          <CardTitle>{day.date}</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-3 gap-4 text-sm">
          <div>
            <div className="text-muted-foreground">Calories</div>
            <div className="text-base font-medium text-foreground">
              {day.totalKcalIn}
            </div>
          </div>
          <div>
            <div className="text-muted-foreground">Workouts</div>
            <div className="text-base font-medium text-foreground">
              {workoutsCount}
            </div>
          </div>
          <div>
            <div className="text-muted-foreground">Sleep</div>
            <div className="text-base font-medium text-foreground">
              {formatMinutes(day.totalSleepMin)}
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}
