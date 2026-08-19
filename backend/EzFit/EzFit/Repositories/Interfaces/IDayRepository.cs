using EzFit.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IDayRepository
    {
        Task<Day?> GetByUserAndDateAsync(int userId, DateOnly date);
        Task<Day> GetOrCreateAsync(int userId, DateOnly date);
        Task<List<Day>> GetRecentByUserAsync(int userId, int count, CancellationToken cancellationToken = default);
    }
}
