using System;
using System.Collections.Generic;

namespace EzFit.DTOs
{
    public class DaySummaryDto
    {
        public DateOnly Date { get; set; }
        public List<EntryDto> Entries { get; set; }
        public int TotalKcalIn { get; set; }
        public int TotalKcalOut { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalFats { get; set; }
        public decimal TotalCarbs { get; set; }
        public int TotalSleepMin { get; set; }
        public int? SleepScore { get; set; }

        public DaySummaryDto(DateOnly date, List<EntryDto> entries, int totalKcalIn,
                             int totalKcalOut, decimal totalProtein, decimal totalFats,
                             decimal totalCarbs, int totalSleepMin, int? sleepScore)
        {
            Date = date;
            Entries = entries;
            TotalKcalIn = totalKcalIn;
            TotalKcalOut = totalKcalOut;
            TotalProtein = totalProtein;
            TotalFats = totalFats;
            TotalCarbs = totalCarbs;
            TotalSleepMin = totalSleepMin;
            SleepScore = sleepScore;
        }
    }
}