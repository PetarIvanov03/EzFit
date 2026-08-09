using EzFit.DTOs.Requests;
using EzFit.DTOs.Responses;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using EzFit.Services.Interfaces;
using EzFit.Services.Mappers;
using System;
using System.Threading.Tasks;

namespace EzFit.Services
{
    public class EntryService : IEntryService
    {
        private readonly IEntryRepository _entryRepository;
        private readonly IDayRepository _dayRepository;

        public EntryService(IEntryRepository entryRepository, IDayRepository dayRepository)
        {
            _entryRepository = entryRepository;
            _dayRepository = dayRepository;
        }
        // Забележка: едно извикване = едно Entry = един тип.
        // Ако AI-ят разпознае няколко събития в една снимка (напр. храна + сън),
        // controller/AI слоят прави отделно извикване на този метод за всяко.
        public async Task<EntryDto> AddEntryAsync(int userId, DateOnly date, CreateEntryDto dto)
        {
            Validate(dto);

            var day = await _dayRepository.GetOrCreateAsync(userId, date);

            var entry = new Entry
            {
                DayId = day.Id,
                Type = dto.Type,
                Title = dto.Title,
                RawText = dto.RawText,
                OccurredAt = dto.OccurredAt,
                CreatedAt = DateTime.UtcNow
            };

            switch (dto.Type)
            {
                case EntryType.Meal:
                    entry.NutritionData = new NutritionData
                    {
                        Kcal = dto.FoodKcal ?? 0,
                        Protein = dto.Protein ?? 0,
                        Fats = dto.Fats ?? 0,
                        Carbs = dto.Carbs ?? 0
                    };
                    break;

                case EntryType.Activity:
                    entry.ActivityData = new ActivityData
                    {
                        Kcal = dto.ActivityKcal ?? 0,
                        DurationMin = dto.DurationMin ?? 0,
                        DistanceKm = dto.DistanceKm,
                        AvgHr = dto.AvgHr,
                        MaxHr = dto.MaxHr,
                        ElevationM = dto.ElevationM,
                        Steps = dto.Steps
                    };
                    break;

                case EntryType.Sleep:
                    entry.SleepData = new SleepData
                    {
                        TotalMin = dto.TotalSleepMin ?? 0,
                        DeepMin = dto.DeepMin,
                        RemMin = dto.RemMin,
                        LightMin = dto.LightMin,
                        Score = dto.SleepScore
                    };
                    break;

                case EntryType.Note:
                    break;
            }

            await _entryRepository.AddAsync(entry);

            return EntryMapper.ToDto(entry);
        }

        private static void Validate(CreateEntryDto dto)
        {
            if (dto.Type == EntryType.Meal)
            {
                if (dto.FoodKcal is > 3000)
                    throw new ArgumentException("Калориите на едно хранене не могат да надвишават 3000.");
                if (dto.FoodKcal is < 0 || dto.Protein is < 0 || dto.Fats is < 0 || dto.Carbs is < 0)
                    throw new ArgumentException("Стойностите не могат да са отрицателни.");
            }

            if (dto.Type == EntryType.Activity)
            {
                if (dto.ActivityKcal is > 5000)
                    throw new ArgumentException("Изгорените калории не могат да надвишават 5000.");
                if (dto.ActivityKcal is < 0 || dto.DurationMin is < 0 || dto.DistanceKm is < 0 || dto.Steps is < 0)
                    throw new ArgumentException("Стойностите не могат да са отрицателни.");
            }

            if (dto.Type == EntryType.Sleep)
            {
                if (dto.TotalSleepMin is < 0 || dto.DeepMin is < 0 || dto.RemMin is < 0 || dto.LightMin is < 0)
                    throw new ArgumentException("Стойностите не могат да са отрицателни.");
            }
        }
    }
}