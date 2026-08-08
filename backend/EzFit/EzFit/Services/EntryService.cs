using EzFit.DTOs;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using EzFit.Services.Interfaces;
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
                        DurationMin = dto.DurationMin ?? 0
                    };
                    break;

                case EntryType.Sleep:
                    entry.SleepData = new SleepData
                    {
                        TotalMin = dto.TotalSleepMin ?? 0
                    };
                    break;

                case EntryType.Note:
                    break; // без разширение
            }

            await _entryRepository.AddAsync(entry);

            return EntryMapper.ToDto(entry);
        }

        private static void Validate(CreateEntryDto dto)
        {
            if (dto.Type == EntryType.Meal && dto.FoodKcal is > 3000)
                throw new ArgumentException("Калориите на едно хранене не могат да надвишават 3000.");

            if (dto.Type == EntryType.Activity && dto.ActivityKcal is > 5000)
                throw new ArgumentException("Изгорените калории не могат да надвишават 5000.");

            if (dto.FoodKcal is < 0 || dto.ActivityKcal is < 0)
                throw new ArgumentException("Калориите не могат да са отрицателни.");
        }
    }
}