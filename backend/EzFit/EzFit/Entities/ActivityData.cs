namespace EzFit.Entities
{
    public class ActivityData
    {
        public int EntryId { get; set; } // PK = FK (1:1)
        public Entry Entry { get; set; } = null!;

        public int Kcal { get; set; }
        public int DurationMin { get; set; }
        public decimal? DistanceKm { get; set; }
        public int? AvgHr { get; set; }
        public int? MaxHr { get; set; }
        public decimal? ElevationM { get; set; }
        public int? Steps { get; set; }
    }
}
