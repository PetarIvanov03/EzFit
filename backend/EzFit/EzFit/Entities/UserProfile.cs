namespace EzFit.Entities
{
    public class UserProfile
    {
        public int UserId { get; set; } // PK = FK (1:1)
        public User User { get; set; } = null!;

        public int Age { get; set; }
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
        public string Gender { get; set; } = string.Empty;

        public int KcalTarget { get; set; }
        public int ProteinTarget { get; set; }
        public int CarbsTarget { get; set; }
        public int FatsTarget { get; set; }
    }
}
