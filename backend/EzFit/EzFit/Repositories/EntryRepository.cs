using EzFit.Data;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EzFit.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        private readonly AppDbContext _context;

        public EntryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Entry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            _context.Entries.Add(entry);
            await _context.SaveChangesAsync();
        }
        public async Task<Entry?> GetByIdAsync(int id)
        {
            return await _context.Entries
                .Include(e => e.NutritionData)
                .Include(e => e.ActivityData)
                .Include(e => e.SleepData)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
