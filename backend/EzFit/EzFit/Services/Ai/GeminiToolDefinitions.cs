namespace EzFit.Services.Ai
{
    public static class GeminiToolDefinitions
    {
        public static object GetTools()
        {
            return new[]
            {
                new
                {
                    functionDeclarations = new object[]
                    {
                        RecordMeal,
                        RecordActivity,
                        RecordSleep,
                        RejectEntry
                    }
                }
            };
        }

        private static readonly object RecordMeal = new
        {
            name = "record_meal",
            description = "Records a meal — called when the user describes or uploads a photo of food with calories/macros.",
            parameters = new
            {
                type = "OBJECT",
                properties = new
                {
                    title = new { type = "STRING", description = "Short title for the meal, e.g. 'Fried eggs'" },
                    raw_text = new { type = "STRING", description = "The user's original description, if any" },
                    occurred_at = new { type = "STRING", description = "ISO 8601 timestamp. If the text/photo references a relative date (yesterday, this morning, 3 days ago) or shows an explicit date, resolve it to an absolute date using the reference date given in the prompt. If no date/time is recognizable at all, omit this field entirely." },
                    food_kcal = new { type = "INTEGER", description = "Total calories of the meal" },
                    protein = new { type = "NUMBER", description = "Protein in grams" },
                    fats = new { type = "NUMBER", description = "Fat in grams" },
                    carbs = new { type = "NUMBER", description = "Carbs in grams" },
                    confidence = new { type = "NUMBER", description = "0.0-1.0, how confident you are in the extraction" },
                    needs_review = new { type = "BOOLEAN", description = "true if the values look unusual and need manual review" }
                },
                required = new[] { "food_kcal", "confidence", "needs_review" }
            }
        };

        private static readonly object RecordActivity = new
        {
            name = "record_activity",
            description = "Records physical activity/workout — called when a workout is described or shown in a screenshot.",
            parameters = new
            {
                type = "OBJECT",
                properties = new
                {
                    title = new { type = "STRING", description = "Short title, e.g. 'Running' or 'Cycling'" },
                    raw_text = new { type = "STRING" },
                    occurred_at = new { type = "STRING", description = "ISO 8601 timestamp. If the text/photo references a relative date (yesterday, this morning, 3 days ago) or shows an explicit date, resolve it to an absolute date using the reference date given in the prompt. If no date/time is recognizable at all, omit this field entirely." },
                    activity_kcal = new { type = "INTEGER", description = "Calories burned" },
                    duration_min = new { type = "INTEGER", description = "Duration in minutes" },
                    distance_km = new { type = "NUMBER" },
                    avg_hr = new { type = "INTEGER", description = "Average heart rate" },
                    max_hr = new { type = "INTEGER", description = "Maximum heart rate" },
                    elevation_m = new { type = "NUMBER", description = "Elevation gain in meters" },
                    steps = new { type = "INTEGER" },
                    confidence = new { type = "NUMBER" },
                    needs_review = new { type = "BOOLEAN" }
                },
                required = new[] { "activity_kcal", "duration_min", "confidence", "needs_review" }
            }
        };

        private static readonly object RecordSleep = new
        {
            name = "record_sleep",
            description = "Records sleep data — called for a Huawei Health screenshot with sleep information. If after a nap, the values are usually a full-day update.",
            parameters = new
            {
                type = "OBJECT",
                properties = new
                {
                    title = new { type = "STRING" },
                    raw_text = new { type = "STRING" },
                    occurred_at = new { type = "STRING", description = "ISO 8601 timestamp. If the text/photo references a relative date (yesterday, this morning, 3 days ago) or shows an explicit date, resolve it to an absolute date using the reference date given in the prompt. If no date/time is recognizable at all, omit this field entirely." },
                    total_sleep_min = new { type = "INTEGER", description = "Total sleep minutes" },
                    deep_min = new { type = "INTEGER" },
                    rem_min = new { type = "INTEGER" },
                    light_min = new { type = "INTEGER" },
                    sleep_score = new { type = "INTEGER" },
                    confidence = new { type = "NUMBER" },
                    needs_review = new { type = "BOOLEAN" }
                },
                required = new[] { "total_sleep_min", "confidence", "needs_review" }
            }
        };

        private static readonly object RejectEntry = new
        {
            name = "reject_entry",
            description = "Called when the input (photo/text) contains no recognizable meal, activity, or sleep data — e.g. an unclear photo, irrelevant content, or a purely informational question with no data to record.",
            parameters = new
            {
                type = "OBJECT",
                properties = new
                {
                    reason = new { type = "STRING", description = "Short explanation of why a valid entry could not be extracted" }
                },
                required = new[] { "reason" }
            }
        };
    }
}