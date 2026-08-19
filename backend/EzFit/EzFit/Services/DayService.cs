using EzFit.DTOs.Responses;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using EzFit.Services.Interfaces;
using EzFit.Services.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class DayService : IDayService
    {
        private readonly IDayRepository _dayRepository;

        public DayService(IDayRepository dayRepository)
        {
            _dayRepository = dayRepository;
        }

        public async Task<DaySummaryDto> GetDaySummaryAsync(int userId, DateOnly date)
        {
            var day = await _dayRepository.GetByUserAndDateAsync(userId, date);
            return BuildSummary(date, day);
        }

        public async Task<List<DaySummaryDto>> GetRecentDaySummariesAsync(int userId, int count, CancellationToken cancellationToken = default)
        {
            var days = await _dayRepository.GetRecentByUserAsync(userId, count, cancellationToken);
            return days.Select(day => BuildSummary(day.Date, day)).ToList();
        }

        private static DaySummaryDto BuildSummary(DateOnly date, Day? day)
        {
            if (day is null)
            {
                return new DaySummaryDto(date, new List<EntryDto>(), 0, 0, 0, 0, 0, 0, null);
            }

            var entryDtos = day.Entries.Select(EntryMapper.ToDto).ToList();

            var totalKcalIn = day.Entries
                .Where(e => e.NutritionData is not null)
                .Sum(e => e.NutritionData!.Kcal);

            var totalKcalOut = day.Entries
                .Where(e => e.ActivityData is not null)
                .Sum(e => e.ActivityData!.Kcal);

            var totalProtein = day.Entries
                .Where(e => e.NutritionData is not null)
                .Sum(e => e.NutritionData!.Protein);

            var totalFats = day.Entries
                .Where(e => e.NutritionData is not null)
                .Sum(e => e.NutritionData!.Fats);

            var totalCarbs = day.Entries
                .Where(e => e.NutritionData is not null)
                .Sum(e => e.NutritionData!.Carbs);

            //! naps?
            var sleepEntry = day.Entries
                .Where(e => e.SleepData is not null)
                .OrderByDescending(e => e.OccurredAt ?? e.CreatedAt)
                .FirstOrDefault();
            var totalSleepMin = sleepEntry?.SleepData?.TotalMin ?? 0;
            var sleepScore = sleepEntry?.SleepData?.Score;

            return new DaySummaryDto(
                date,
                entryDtos,
                totalKcalIn,
                totalKcalOut,
                totalProtein,
                totalFats,
                totalCarbs,
                totalSleepMin,
                sleepScore);
        }
    }
}