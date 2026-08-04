using EzFit.Entities;
using System;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IDayRepository
    {
        Task<Day?> GetByUserAndDateAsync(int userId, DateOnly date);
        Task<Day> GetOrCreateAsync(int userId, DateOnly date);
    }
}
