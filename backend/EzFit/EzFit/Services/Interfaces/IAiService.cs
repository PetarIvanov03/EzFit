using EzFit.DTOs.Ai;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IAiService
    {
        Task<List<AiExtractionResult>> ExtractAsync(string? message, List<byte[]> images);
    }
}
