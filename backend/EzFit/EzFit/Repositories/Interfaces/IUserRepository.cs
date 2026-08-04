using EzFit.Entities;
using System.Threading.Tasks;

namespace EzFit.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
    }
}
