namespace EzFit.Options
{
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        public RateLimitPolicyOptions Log { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };
        public RateLimitPolicyOptions Api { get; set; } = new() { PermitLimit = 60, WindowSeconds = 60 };
    }

    public class RateLimitPolicyOptions
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
    }
}
