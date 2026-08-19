using System;

namespace EzFit.Exceptions
{
    // Thrown when Gemini responds 429 (request-per-day/request-per-minute quota
    // exhausted). Distinct from AiServiceException so this maps to 429 — a state the
    // client can meaningfully retry later — instead of 502, which reads as our bug.
    public class GeminiRateLimitException : Exception
    {
        public GeminiRateLimitException(string message) : base(message) { }
    }
}
