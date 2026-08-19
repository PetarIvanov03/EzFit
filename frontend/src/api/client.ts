import axios from "axios"

const FALLBACK_API_URL = "https://localhost:7059/api"

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? FALLBACK_API_URL,
})
