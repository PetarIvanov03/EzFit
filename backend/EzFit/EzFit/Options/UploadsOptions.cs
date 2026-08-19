namespace EzFit.Options
{
    public class UploadsOptions
    {
        public const string SectionName = "Uploads";

        public long MaxFileSizeBytes { get; set; }
        public int MaxFileCount { get; set; }
        public long MaxPixels { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int MaxTilesPerRequest { get; set; }
    }
}
