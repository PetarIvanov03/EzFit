import { useMutation, useQueryClient } from "@tanstack/react-query"
import { apiClient } from "@/api/client"
import type { LogResultDto } from "@/types/entry"

export interface LogEntryInput {
  message?: string
  images?: File[]
}

export function useLogEntry() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ message, images }: LogEntryInput) => {
      const formData = new FormData()
      if (message) formData.append("message", message)
      images?.forEach((file) => formData.append("images", file))

      // Do not set Content-Type manually — the browser needs to add its own
      // multipart boundary, which a hardcoded header would clobber.
      const { data } = await apiClient.post<LogResultDto>("/log", formData)
      return data
    },
    onSuccess: () => {
      // The backend resolves the day itself from AI-extracted occurred_at (falling
      // back to today), so the client has no date to key an invalidation on — a
      // single submission can even land entries on several different days.
      // Invalidate every "day" query broadly rather than guessing which one(s).
      queryClient.invalidateQueries({ queryKey: ["day"] })
      queryClient.invalidateQueries({ queryKey: ["days", "list"] })
    },
  })
}
