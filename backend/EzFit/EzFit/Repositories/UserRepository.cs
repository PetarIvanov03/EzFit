using EzFit.Data;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FindAsync(new object?[] { id }, cancellationToken);
        }
    }
}
