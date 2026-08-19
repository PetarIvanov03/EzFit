using EzFit.Data;
using EzFit.Middleware;
using EzFit.Options;
using EzFit.Repositories;
using EzFit.Repositories.Interfaces;
using EzFit.Services;
using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using System;
using System.Threading.RateLimiting;

namespace EzFit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IDayRepository, DayRepository>();
            builder.Services.AddScoped<IEntryRepository, EntryRepository>();

            builder.Services.AddScoped<IDayService, DayService>();
            builder.Services.AddScoped<IEntryService, EntryService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IAiService, GeminiAiService>();
            builder.Services.AddScoped<ICurrentUserProvider, StaticCurrentUserProvider>();

            builder.Services.Configure<UploadsOptions>(builder.Configuration.GetSection(UploadsOptions.SectionName));
            builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));
            builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
            builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
            builder.Services.Configure<CurrentUserOptions>(builder.Configuration.GetSection(CurrentUserOptions.SectionName));
            builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));

            var uploadsOptions = builder.Configuration.GetSection(UploadsOptions.SectionName).Get<UploadsOptions>()
                ?? new UploadsOptions();
            var maxRequestBodyBytes = uploadsOptions.MaxFileSizeBytes * uploadsOptions.MaxFileCount;

            // A whole decoded frame is one contiguous allocation; cap it well above
            // MaxPixels (raw RGBA32) so legitimate uploads still decode, but bound the
            // worst case a malicious/corrupt file can force onto the 512 MB container.
            // Floor keeps small configs usable; ceiling means a future config edit can't
            // push this past what the container can survive alongside everything else.
            //
            // At MaxPixels = 50M (raised to comfortably clear a 4032x3024 phone photo),
            // this computes to 50M * 4 * 2 = 400,000,000 bytes ~= 381 MB, so the clamp
            // to the 256 MB ceiling binds again — the derivation is "live" in the sense
            // that it reacts to MaxPixels, but above ~33.5M pixels it's dead weight: any
            // MaxPixels from here up produces the same 256 MB result. Left as-is rather
            // than raising the ceiling to match: 256 MB was already sized as the worst
            // case this container can spend on ONE decode buffer alongside the .NET/
            // ASP.NET Core runtime baseline (~100-150 MB), the Npgsql pool, and the WebP
            // byte arrays LogController buffers for the Gemini call — raising it further
            // erodes that margin for a case (uploads near 50M pixels) that's rare relative
            // to the typical phone photo or tall screenshot this app actually receives.
            const int BytesPerPixel = 4;
            const int DecodeSafetyFactor = 2;
            const int MinAllocationLimitMb = 64;
            const int MaxAllocationLimitMb = 256;
            var allocationLimitMb = (int)Math.Clamp(
                uploadsOptions.MaxPixels * BytesPerPixel * DecodeSafetyFactor / (1024 * 1024),
                MinAllocationLimitMb,
                MaxAllocationLimitMb);

            Configuration.Default.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions
            {
                AllocationLimitMegabytes = allocationLimitMb
            });

            // Kestrel/form limits sized off the same Uploads config the controller and
            // ImageService validate against, so there's one place to change them.
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = maxRequestBodyBytes;
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = maxRequestBodyBytes;
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions
            .CommandTimeout(30)
            .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));

            builder.Services.AddHttpClient("Gemini", client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://ez-fit.vercel.app")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var rateLimitingOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
                ?? new RateLimitingOptions();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
                };

                // Partitioned on the (forwarded-header-corrected) client IP — see UseForwardedHeaders below.
                options.AddPolicy("log", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingOptions.Log.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingOptions.Log.WindowSeconds),
                        QueueLimit = 0
                    }));

                options.AddPolicy("api", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingOptions.Api.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingOptions.Api.WindowSeconds),
                        QueueLimit = 0
                    }));
            });

            var app = builder.Build();

            if (string.IsNullOrEmpty(builder.Configuration[$"{SecurityOptions.SectionName}:ApiKey"]))
            {
                app.Logger.LogWarning(
                    "Security:ApiKey is not configured — the /api endpoints are not protected by the shared API key gate.");
            }

            // Checked against the raw config key, not the bound GeminiOptions.Model value —
            // comparing the resolved value to GeminiOptions.FallbackModel would misfire if
            // someone ever configures that exact string on purpose.
            if (string.IsNullOrWhiteSpace(builder.Configuration[$"{GeminiOptions.SectionName}:Model"]))
            {
                app.Logger.LogWarning(
                    "Gemini:Model is not configured — falling back to the hardcoded default '{FallbackModel}'.",
                    GeminiOptions.FallbackModel);
            }

            // Render terminates TLS and forwards plain HTTP; this must run before anything
            // that reads the scheme (HTTPS redirection) or the client IP (rate limiter).
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("Frontend");

            app.UseMiddleware<ApiKeyMiddleware>();

            app.UseRateLimiter();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static string GetClientIp(HttpContext httpContext) =>
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
