using EzFit.DTOs.Ai;
using EzFit.DTOs.Requests;
using EzFit.Entities;

namespace EzFit.Services.Mappers
{
    public static class AiResponseMapper
    {
        // null резултат = reject_entry, orchestrator-ът проверява това
        // и не вика EntryService за този резултат
        public static CreateEntryDto? ToCreateEntryDto(AiExtractionResult result)
        {
            if (result.ToolType == AiToolType.RejectEntry)
                return null;

            var dto = new CreateEntryDto
            {
                Type = MapToolTypeToEntryType(result.ToolType),
                Title = result.Title,
                RawText = result.RawText,
                OccurredAt = result.OccurredAt
            };

            switch (result.ToolType)
            {
                case AiToolType.RecordMeal:
                    dto.FoodKcal = result.FoodKcal;
                    dto.Protein = result.Protein;
                    dto.Fats = result.Fats;
                    dto.Carbs = result.Carbs;
                    break;

                case AiToolType.RecordActivity:
                    dto.ActivityKcal = result.ActivityKcal;
                    dto.DurationMin = result.DurationMin;
                    dto.DistanceKm = result.DistanceKm;
                    dto.AvgHr = result.AvgHr;
                    dto.MaxHr = result.MaxHr;
                    dto.ElevationM = result.ElevationM;
                    dto.Steps = result.Steps;
                    break;

                case AiToolType.RecordSleep:
                    dto.TotalSleepMin = result.TotalSleepMin;
                    dto.DeepMin = result.DeepMin;
                    dto.RemMin = result.RemMin;
                    dto.LightMin = result.LightMin;
                    dto.SleepScore = result.SleepScore;
                    break;
            }

            return dto;
        }

        private static EntryType MapToolTypeToEntryType(AiToolType toolType)
        {
            return toolType switch
            {
                AiToolType.RecordMeal => EntryType.Meal,
                AiToolType.RecordActivity => EntryType.Activity,
                AiToolType.RecordSleep => EntryType.Sleep,
                _ => EntryType.Note
            };
        }
    }
}