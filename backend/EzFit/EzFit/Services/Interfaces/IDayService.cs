using EzFit.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IDayService
    {
        Task<DaySummaryDto> GetDaySummaryAsync(int userId, DateOnly date);
        Task<List<DaySummaryDto>> GetRecentDaySummariesAsync(int userId, int count, CancellationToken cancellationToken = default);
    }
}
