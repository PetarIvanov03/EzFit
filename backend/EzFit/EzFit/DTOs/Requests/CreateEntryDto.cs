using EzFit.Entities;
using System;

namespace EzFit.DTOs.Requests
{
    public class CreateEntryDto
    {
        public EntryType Type { get; set; }
        public string? Title { get; set; }
        public string? RawText { get; set; }
        public DateTime? OccurredAt { get; set; }

        // Meal
        public int? FoodKcal { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Fats { get; set; }
        public decimal? Carbs { get; set; }

        // Activity
        public int? ActivityKcal { get; set; }
        public int? DurationMin { get; set; }
        public decimal? DistanceKm { get; set; }
        public int? AvgHr { get; set; }
        public int? MaxHr { get; set; }
        public decimal? ElevationM { get; set; }
        public int? Steps { get; set; }

        // Sleep
        public int? TotalSleepMin { get; set; }
        public int? DeepMin { get; set; }
        public int? RemMin { get; set; }
        public int? LightMin { get; set; }
        public int? SleepScore { get; set; }
    }
}