using System;

namespace EzFit.Entities
{
    public class Entry
    {
        public int Id { get; set; }
        public int DayId { get; set; }
        public Day Day { get; set; } = null!;

        public EntryType Type { get; set; }
        public string? Title { get; set; }
        public string? RawText { get; set; }
        public string? ImagePath { get; set; }
        public string? AiRawResponse { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? OccurredAt { get; set; }

        // 1:1 разширения — само едно от тях е попълнено, според Type
        public NutritionData? NutritionData { get; set; }
        public ActivityData? ActivityData { get; set; }
        public SleepData? SleepData { get; set; }
    }
}
