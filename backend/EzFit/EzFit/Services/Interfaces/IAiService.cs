using EzFit.DTOs.Ai;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Services.Interfaces
{
    public interface IAiService
    {
        Task<AiExtractionResponse> ExtractAsync(string? message, List<byte[]> images, CancellationToken cancellationToken = default);
    }
}
