using EzFit.DTOs.Ai;
using EzFit.Services.Ai;
using EzFit.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GeminiAiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AiExtractionResponse> ExtractAsync(string? message, List<byte[]> images)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-3.5-flash";

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Gemini:ApiKey is missing.");

            // Build the "parts" array: optional text first, then one part per image
            var parts = new List<object>();

            if (!string.IsNullOrWhiteSpace(message))
            {
                parts.Add(new { text = message });
            }

            foreach (var imageBytes in images)
            {
                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = "image/webp",
                        data = Convert.ToBase64String(imageBytes)
                    }
                });
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                tools = GeminiToolDefinitions.GetTools()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("Gemini");
            var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{model}:generateContent")
            {
                Content = content
            };
            request.Headers.Add("x-goog-api-key", apiKey);

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini request failed ({response.StatusCode}): {responseBody}");

            return new AiExtractionResponse
            {
                RawResponseJson = responseBody,
                Results = ParseFunctionCalls(responseBody)
            };
        }

        private static List<AiExtractionResult> ParseFunctionCalls(string responseBody)
        {
            var results = new List<AiExtractionResult>();

            using var doc = JsonDocument.Parse(responseBody);
            var responseParts = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts");

            foreach (var part in responseParts.EnumerateArray())
            {
                if (!part.TryGetProperty("functionCall", out var functionCall))
                    continue; // this part is plain text, not a tool call — skip it

                var name = functionCall.GetProperty("name").GetString();
                var args = functionCall.GetProperty("args");

                var result = new AiExtractionResult
                {
                    ToolType = MapToolName(name),
                    Confidence = GetOptionalDecimal(args, "confidence"),
                    NeedsReview = GetOptionalBool(args, "needs_review") ?? false,
                    RejectionReason = GetOptionalString(args, "reason"),

                    Title = GetOptionalString(args, "title"),
                    RawText = GetOptionalString(args, "raw_text"),
                    OccurredAt = GetOptionalDateTime(args, "occurred_at"),

                    FoodKcal = GetOptionalInt(args, "food_kcal"),
                    Protein = GetOptionalDecimal(args, "protein"),
                    Fats = GetOptionalDecimal(args, "fats"),
                    Carbs = GetOptionalDecimal(args, "carbs"),

                    ActivityKcal = GetOptionalInt(args, "activity_kcal"),
                    DurationMin = GetOptionalInt(args, "duration_min"),
                    DistanceKm = GetOptionalDecimal(args, "distance_km"),
                    AvgHr = GetOptionalInt(args, "avg_hr"),
                    MaxHr = GetOptionalInt(args, "max_hr"),
                    ElevationM = GetOptionalDecimal(args, "elevation_m"),
                    Steps = GetOptionalInt(args, "steps"),

                    TotalSleepMin = GetOptionalInt(args, "total_sleep_min"),
                    DeepMin = GetOptionalInt(args, "deep_min"),
                    RemMin = GetOptionalInt(args, "rem_min"),
                    LightMin = GetOptionalInt(args, "light_min"),
                    SleepScore = GetOptionalInt(args, "sleep_score")
                };

                results.Add(result);
            }

            return results;
        }

        private static AiToolType MapToolName(string? name)
        {
            return name switch
            {
                "record_meal" => AiToolType.RecordMeal,
                "record_activity" => AiToolType.RecordActivity,
                "record_sleep" => AiToolType.RecordSleep,
                "reject_entry" => AiToolType.RejectEntry,
                _ => throw new InvalidOperationException($"Unknown tool name returned by Gemini: {name}")
            };
        }

        private static string? GetOptionalString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
        }

        private static int? GetOptionalInt(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : null;
        }

        private static decimal? GetOptionalDecimal(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetDecimal()
                : null;
        }

        private static bool? GetOptionalBool(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var value) &&
                   (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                ? value.GetBoolean()
                : null;
        }

        private static DateTime? GetOptionalDateTime(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
                return null;

            return DateTime.TryParse(value.GetString(), out var parsed) ? parsed : null;
        }
    }
}
