import { useEffect } from "react"

// Captured once at module load, i.e. from index.html's static <title> before any
// page has had a chance to override it — the single source of truth for "no page
// is active" rather than a second hardcoded "EzFit" string that could drift from it.
const BASE_TITLE = document.title

// Two effects, not one: route components can stay mounted across navigations to the
// same route with different params (e.g. Link-ing from one day to another day both
// matching "days/:date"), so the title has to update on every `title` change, not
// just once on mount. Restoring BASE_TITLE is a separate effect that only fires on
// actual unmount (empty deps) — folding it into the first effect's cleanup would
// restore-then-immediately-overwrite on every param change instead of just at the end.
export function usePageTitle(title: string) {
  useEffect(() => {
    document.title = title
  }, [title])

  useEffect(() => {
    return () => {
      document.title = BASE_TITLE
    }
  }, [])
}
