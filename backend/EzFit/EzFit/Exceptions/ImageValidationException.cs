using System;

namespace EzFit.Exceptions
{
    // Thrown when an uploaded image fails validation before decoding
    // (e.g. header-reported dimensions exceed configured limits).
    public class ImageValidationException : Exception
    {
        public ImageValidationException(string message) : base(message) { }
        public ImageValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
