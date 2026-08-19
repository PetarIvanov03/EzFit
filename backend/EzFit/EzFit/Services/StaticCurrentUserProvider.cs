using EzFit.Options;
using EzFit.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EzFit.Services
{
    public class StaticCurrentUserProvider : ICurrentUserProvider
    {
        public StaticCurrentUserProvider(IOptions<CurrentUserOptions> options)
        {
            UserId = options.Value.Id;
        }

        public int UserId { get; }
    }
}
