import { EntryType } from "@/types/entry"

export const entryTypeStyles: Record<
  EntryType,
  { label: string; border: string; badge: string }
> = {
  [EntryType.Meal]: {
    label: "Meal",
    border: "border-l-amber-500",
    badge: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-400",
  },
  [EntryType.Activity]: {
    label: "Activity",
    border: "border-l-emerald-500",
    badge:
      "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-400",
  },
  [EntryType.Sleep]: {
    label: "Sleep",
    border: "border-l-indigo-500",
    badge:
      "bg-indigo-100 text-indigo-800 dark:bg-indigo-500/15 dark:text-indigo-400",
  },
  [EntryType.Note]: {
    label: "Note",
    border: "border-l-slate-400",
    badge: "bg-slate-100 text-slate-700 dark:bg-slate-500/15 dark:text-slate-400",
  },
}

export const entryTypeOrder: EntryType[] = [
  EntryType.Meal,
  EntryType.Activity,
  EntryType.Sleep,
  EntryType.Note,
]
