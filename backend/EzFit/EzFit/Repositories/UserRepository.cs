using EzFit.Data;
using EzFit.Entities;
using EzFit.Repositories.Interfaces;
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

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}
