using System.Collections.Generic;

namespace EzFit.DTOs.Responses
{
    public class LogResultDto
    {
        public List<EntryDto> CreatedEntries { get; set; } = new();
        public List<string> RejectionReasons { get; set; } = new();
    }
}