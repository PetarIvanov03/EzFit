namespace EzFit.Entities
{
    public class SleepData
    {
        public int EntryId { get; set; } // PK = FK (1:1)
        public Entry Entry { get; set; } = null!;

        public int TotalMin { get; set; }
        public int? DeepMin { get; set; }
        public int? RemMin { get; set; }
        public int? LightMin { get; set; }
        public int? Score { get; set; }
    }
}
