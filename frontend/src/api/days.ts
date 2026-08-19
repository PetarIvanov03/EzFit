import { useQuery } from "@tanstack/react-query"
import { apiClient } from "@/api/client"
import type { DaySummaryDto } from "@/types/entry"

export function useDayList(count = 7) {
  return useQuery({
    queryKey: ["days", "list", count],
    queryFn: async () => {
      const { data } = await apiClient.get<DaySummaryDto[]>("/day/list", {
        params: { count },
      })
      return data
    },
  })
}

export function useDay(date: string) {
  return useQuery({
    queryKey: ["day", date],
    queryFn: async () => {
      const { data } = await apiClient.get<DaySummaryDto>("/day", {
        params: { date },
      })
      return data
    },
    enabled: date.length > 0,
  })
}
