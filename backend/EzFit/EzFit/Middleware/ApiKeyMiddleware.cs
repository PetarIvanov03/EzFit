using EzFit.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EzFit.Middleware
{
    // Stopgap until real auth: gates /api behind a shared key shipped in the frontend
    // bundle. Not a secret boundary — just stops casual scripted abuse of Gemini quota.
    public class ApiKeyMiddleware
    {
        private const string HeaderName = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly string? _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<SecurityOptions> options)
        {
            _next = next;
            _apiKey = options.Value.ApiKey;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (string.IsNullOrEmpty(_apiKey) || !context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            var provided = context.Request.Headers[HeaderName].ToString();

            if (!IsValidKey(provided))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing or invalid API key.");
                return;
            }

            await _next(context);
        }

        private bool IsValidKey(string provided)
        {
            var providedBytes = Encoding.UTF8.GetBytes(provided);
            var expectedBytes = Encoding.UTF8.GetBytes(_apiKey!);

            // Lengths must match before FixedTimeEquals (it throws on mismatched spans);
            // the length compare itself is a negligible timing signal next to the key check it guards.
            return providedBytes.Length == expectedBytes.Length
                && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
    }
}
