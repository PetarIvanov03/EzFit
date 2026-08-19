using System;

namespace EzFit.Exceptions
{
    // Thrown when the upstream AI provider call fails. Message is status-code-only —
    // the response body is logged where it's caught, never surfaced to the client.
    public class AiServiceException : Exception
    {
        public AiServiceException(string message) : base(message) { }
    }
}
