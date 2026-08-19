namespace EzFit.Options
{
    public class ImageStorageOptions
    {
        public const string SectionName = "ImageStorage";

        public string Format { get; set; } = "Webp";
        public int Quality { get; set; } = 80;
        public string UploadsRoot { get; set; } = "App_Data/uploads";
    }
}
