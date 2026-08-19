export const EntryType = {
  Meal: 0,
  Activity: 1,
  Sleep: 2,
  Note: 3,
} as const

export type EntryType = (typeof EntryType)[keyof typeof EntryType]

export interface EntryDto {
  id: number
  type: EntryType
  title: string | null
  occurredAt: string | null
  createdAt: string

  // Meal (type === EntryType.Meal)
  foodKcal: number | null
  protein: number | null
  fats: number | null
  carbs: number | null

  // Activity (type === EntryType.Activity)
  activityKcal: number | null
  durationMin: number | null
  distanceKm: number | null
  avgHr: number | null
  maxHr: number | null
  elevationM: number | null
  steps: number | null

  // Sleep (type === EntryType.Sleep)
  totalSleepMin: number | null
  deepMin: number | null
  remMin: number | null
  lightMin: number | null
  sleepScore: number | null
}

export interface DaySummaryDto {
  date: string
  entries: EntryDto[]
  totalKcalIn: number
  totalKcalOut: number
  totalProtein: number
  totalFats: number
  totalCarbs: number
  totalSleepMin: number
  sleepScore: number | null
}

export interface LogResultDto {
  createdEntries: EntryDto[]
  rejectionReasons: string[]
}
