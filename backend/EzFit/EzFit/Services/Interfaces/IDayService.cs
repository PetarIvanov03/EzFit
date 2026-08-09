using EzFit.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IDayService
    {
        Task<DaySummaryDto> GetDaySummaryAsync(int userId, DateOnly date);
    }
}
