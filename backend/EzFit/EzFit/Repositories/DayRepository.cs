using EzFit.Data;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Repositories
{
    public class DayRepository : IDayRepository
    {
        private readonly AppDbContext _context;

        public DayRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Day?> GetByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _context.Days
                .Include(d => d.Entries)
                    .ThenInclude(e => e.NutritionData)
                .Include(d => d.Entries)
                    .ThenInclude(e => e.ActivityData)
                .Include(d => d.Entries)
                    .ThenInclude(e => e.SleepData)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Date == date, cancellationToken);
        }

        public async Task<Day> GetOrCreateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default)
        {
            var day = await GetByUserAndDateAsync(userId, date, cancellationToken);

            if (day is null)
            {
                day = new Day { UserId = userId, Date = date };
                _context.Days.Add(day);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return day;
        }

        public async Task<List<Day>> GetRecentByUserAsync(int userId, int count, CancellationToken cancellationToken = default)
        {
            return await _context.Days
                .Include(d => d.Entries)
                    .ThenInclude(e => e.NutritionData)
                .Include(d => d.Entries)
                    .ThenInclude(e => e.ActivityData)
                .Include(d => d.Entries)
                    .ThenInclude(e => e.SleepData)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.Date)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
