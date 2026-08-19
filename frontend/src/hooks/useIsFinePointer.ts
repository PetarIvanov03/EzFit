import { useEffect, useState } from "react"

const QUERY = "(hover: hover) and (pointer: fine)"

function readMatch() {
  if (typeof window === "undefined" || typeof window.matchMedia !== "function") return false
  return window.matchMedia(QUERY).matches
}

// Enter-to-submit only makes sense where Shift+Enter is reachable to insert a
// newline instead — mobile keyboards have no Shift key, so touch users need
// Enter to just type. Pointer/hover capability is a more reliable signal for
// "has a physical keyboard" than screen size or user-agent sniffing, and it
// stays correct if a mouse/keyboard is attached or detached mid-session.
export function useIsFinePointer(): boolean {
  const [isFinePointer, setIsFinePointer] = useState(readMatch)

  useEffect(() => {
    if (typeof window.matchMedia !== "function") return

    const mql = window.matchMedia(QUERY)
    const handleChange = (e: MediaQueryListEvent) => setIsFinePointer(e.matches)

    mql.addEventListener("change", handleChange)
    return () => mql.removeEventListener("change", handleChange)
  }, [])

  return isFinePointer
}
