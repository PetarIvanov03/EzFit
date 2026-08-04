using EzFit.Entities;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IEntryRepository
    {
        Task AddAsync(Entry entry);
        Task<Entry?> GetByIdAsync(int id);
    }
}
