using EzFit.DTOs;
using EzFit.Entities;

namespace EzFit.Services
{
    public static class EntryMapper
    {
        public static EntryDto ToDto(Entry entry)
        {
            var dto = new EntryDto
            {
                Id = entry.Id,
                Type = entry.Type,
                Title = entry.Title,
                OccurredAt = entry.OccurredAt,
                CreatedAt = entry.CreatedAt
            };

            if (entry.NutritionData is not null)
            {
                dto.FoodKcal = entry.NutritionData.Kcal;
                dto.Protein = entry.NutritionData.Protein;
                dto.Fats = entry.NutritionData.Fats;
                dto.Carbs = entry.NutritionData.Carbs;
            }

            if (entry.ActivityData is not null)
            {
                dto.ActivityKcal = entry.ActivityData.Kcal;
                dto.DurationMin = entry.ActivityData.DurationMin;
                dto.DistanceKm = entry.ActivityData.DistanceKm;
                dto.AvgHr = entry.ActivityData.AvgHr;
                dto.MaxHr = entry.ActivityData.MaxHr;
                dto.ElevationM = entry.ActivityData.ElevationM;
                dto.Steps = entry.ActivityData.Steps;
            }

            if (entry.SleepData is not null)
            {
                dto.TotalMin = entry.SleepData.TotalMin;
                dto.DeepMin = entry.SleepData.DeepMin;
                dto.RemMin = entry.SleepData.RemMin;
                dto.LightMin = entry.SleepData.LightMin;
                dto.Score = entry.SleepData.Score;
            }

            return dto;
        }
    }
}