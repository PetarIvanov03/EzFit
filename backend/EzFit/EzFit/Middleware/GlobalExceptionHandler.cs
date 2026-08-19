using EzFit.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private static readonly Dictionary<Type, int> StatusCodesByExceptionType = new()
        {
            [typeof(ImageValidationException)] = StatusCodes.Status400BadRequest,
            [typeof(AiServiceException)] = StatusCodes.Status502BadGateway,
            [typeof(GeminiRateLimitException)] = StatusCodes.Status429TooManyRequests,
        };

        private readonly IProblemDetailsService _problemDetailsService;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            IHostEnvironment environment,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _environment = environment;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = StatusCodesByExceptionType.GetValueOrDefault(exception.GetType(), StatusCodes.Status500InternalServerError);

            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = statusCode;

            // Validation (400) and quota (429) messages are deliberately written to be
            // safe/actionable for the client, so they're surfaced in every environment.
            // Everything else only gets a detail in Development — never in prod, never a
            // stack trace, since those messages weren't written with an end user in mind.
            var detail = statusCode == StatusCodes.Status400BadRequest || statusCode == StatusCodes.Status429TooManyRequests
                ? exception.Message
                : _environment.IsDevelopment() ? exception.ToString() : null;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Detail = detail
                }
            });
        }
    }
}
