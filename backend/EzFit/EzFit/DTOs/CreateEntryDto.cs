using EzFit.Entities;

namespace EzFit.DTOs
{
    public class CreateEntryDto
    {
        public EntryType Type { get; set; }
        public string? Title { get; set; }
        public string? RawText { get; set; }

        public int? FoodKcal { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Fats { get; set; }
        public decimal? Carbs { get; set; }

        public int? ActivityKcal { get; set; }
        public int? DurationMin { get; set; }

        public int? TotalSleepMin { get; set; }
    }
}