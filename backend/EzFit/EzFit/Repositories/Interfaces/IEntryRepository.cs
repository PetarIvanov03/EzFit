using EzFit.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IEntryRepository
    {
        Task AddAsync(Entry entry, CancellationToken cancellationToken = default);
        Task<Entry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
