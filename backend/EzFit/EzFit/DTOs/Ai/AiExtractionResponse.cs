using System.Collections.Generic;

namespace EzFit.DTOs.Ai
{
    public class AiExtractionResponse
    {
        public string RawResponseJson { get; set; } = string.Empty;
        public List<AiExtractionResult> Results { get; set; } = new();
    }
}