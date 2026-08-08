using EzFit.DTOs;
using System;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IEntryService
    {
        Task<EntryDto> AddEntryAsync(int userId, DateOnly date, CreateEntryDto dto);
    }
}
