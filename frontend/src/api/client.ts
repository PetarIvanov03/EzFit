import axios from "axios"

const FALLBACK_API_URL = "https://localhost:7059/api"

// Not a secret — ships in the public bundle. Stopgap against casual scripted abuse
// until real auth lands, not a substitute for it.
const apiKey = import.meta.env.VITE_API_KEY as string | undefined

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? FALLBACK_API_URL,
  headers: apiKey ? { "X-Api-Key": apiKey } : undefined,
})
