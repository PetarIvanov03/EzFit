using EzFit.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
