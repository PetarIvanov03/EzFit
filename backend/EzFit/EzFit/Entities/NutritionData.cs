namespace EzFit.Entities
{
    public class NutritionData
    {
        public int EntryId { get; set; } // PK = FK (1:1)
        public Entry Entry { get; set; } = null!;

        public int Kcal { get; set; }
        public decimal Protein { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbs { get; set; }
    }
}
