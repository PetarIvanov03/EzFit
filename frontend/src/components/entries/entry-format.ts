import { formatMinutes } from "@/lib/format"
import { EntryType, type EntryDto } from "@/types/entry"

export interface EntryDisplay {
  detail: string
  metrics: string[]
}

export function formatEntry(entry: EntryDto): EntryDisplay {
  switch (entry.type) {
    case EntryType.Meal: {
      const parts: string[] = []
      if (entry.protein !== null) parts.push(`${entry.protein}g protein`)
      if (entry.fats !== null) parts.push(`${entry.fats}g fat`)
      if (entry.carbs !== null) parts.push(`${entry.carbs}g carbs`)
      return {
        detail: parts.join(" · ") || "—",
        metrics: entry.foodKcal !== null ? [`${entry.foodKcal} kcal`] : [],
      }
    }
    case EntryType.Activity: {
      const parts: string[] = []
      if (entry.durationMin !== null) parts.push(`${entry.durationMin} min`)
      if (entry.distanceKm !== null) parts.push(`${entry.distanceKm} km`)
      if (entry.steps !== null) parts.push(`${entry.steps} steps`)
      return {
        detail: parts.join(" · ") || "—",
        metrics: entry.activityKcal !== null ? [`${entry.activityKcal} kcal`] : [],
      }
    }
    case EntryType.Sleep: {
      const parts: string[] = []
      if (entry.deepMin !== null) parts.push(`${entry.deepMin}m deep`)
      if (entry.remMin !== null) parts.push(`${entry.remMin}m REM`)
      if (entry.lightMin !== null) parts.push(`${entry.lightMin}m light`)
      const metrics: string[] = []
      if (entry.totalSleepMin !== null) metrics.push(formatMinutes(entry.totalSleepMin))
      if (entry.sleepScore !== null) metrics.push(`Score ${entry.sleepScore}`)
      return { detail: parts.join(" · ") || "—", metrics }
    }
    case EntryType.Note:
    default:
      return { detail: "", metrics: [] }
  }
}
