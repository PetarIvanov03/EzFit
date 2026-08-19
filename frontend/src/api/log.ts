import { useMutation, useQueryClient } from "@tanstack/react-query"
import { apiClient } from "@/api/client"
import type { LogResultDto } from "@/types/entry"

export interface LogEntryInput {
  date: string
  message?: string
  images?: File[]
}

export function useLogEntry() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ date, message, images }: LogEntryInput) => {
      const formData = new FormData()
      if (message) formData.append("message", message)
      images?.forEach((file) => formData.append("images", file))

      // Do not set Content-Type manually — the browser needs to add its own
      // multipart boundary, which a hardcoded header would clobber.
      const { data } = await apiClient.post<LogResultDto>("/log", formData, {
        params: { date },
      })
      return data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["day", variables.date] })
      queryClient.invalidateQueries({ queryKey: ["days", "list"] })
    },
  })
}
