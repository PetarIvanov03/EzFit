namespace EzFit.Options
{
    public class GeminiOptions
    {
        public const string SectionName = "Gemini";

        // Only used if the "Gemini:Model" config key is entirely absent (e.g. appsettings.json
        // itself is missing/corrupt) — normal operation always gets Model from configuration,
        // which Program.cs warns about at startup if that's not the case.
        public const string FallbackModel = "gemini-3.5-flash-lite";

        public string? ApiKey { get; set; }
        public string Model { get; set; } = FallbackModel;
    }
}
