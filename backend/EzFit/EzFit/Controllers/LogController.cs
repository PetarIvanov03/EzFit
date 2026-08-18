using EzFit.DTOs.Ai;
using EzFit.DTOs.Requests;
using EzFit.DTOs.Responses;
using EzFit.Services.Interfaces;
using EzFit.Services.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAiService _aiService;
        private readonly IEntryService _entryService;

        private const int HardcodedUserId = 1;

        public LogController(
            IImageService imageService,
            IFileStorageService fileStorageService,
            IAiService aiService,
            IEntryService entryService)
        {
            _imageService = imageService;
            _fileStorageService = fileStorageService;
            _aiService = aiService;
            _entryService = entryService;
        }

        // POST api/log?date=2026-08-18  (form-data: message=<text>, images=<file(s)>)
        [HttpPost]
        public async Task<ActionResult<LogResultDto>> Log(
            [FromQuery] DateOnly date,
            [FromForm] string? message,
            [FromForm] List<IFormFile>? images)
        {
            var imageBytesForAi = new List<byte[]>();
            var savedBaseNames = new List<string>();

            if (images is not null)
            {
                foreach (var image in images)
                {
                    var tiles = await _imageService.ProcessAsync(image.OpenReadStream());

                    // Encode a copy for the AI call before FileStorageService disposes the tiles
                    foreach (var tile in tiles)
                    {
                        using var ms = new MemoryStream();
                        await tile.SaveAsync(ms, new WebpEncoder());
                        imageBytesForAi.Add(ms.ToArray());
                    }

                    var baseName = _fileStorageService.GenerateBaseName(HardcodedUserId);
                    await _fileStorageService.SaveAsync(baseName, tiles); // disposes tiles internally

                    savedBaseNames.Add(baseName);
                }
            }

            var aiResponse = await _aiService.ExtractAsync(message, imageBytesForAi);
            var imagePath = savedBaseNames.Count > 0 ? string.Join(",", savedBaseNames) : null;

            var result = new LogResultDto();

            foreach (var extraction in aiResponse.Results)
            {
                var dto = AiResponseMapper.ToCreateEntryDto(extraction);

                if (dto is null)
                {
                    result.RejectionReasons.Add(extraction.RejectionReason ?? "Unrecognized input.");
                    continue;
                }

                dto.ImagePath = imagePath;
                dto.AiRawResponse = aiResponse.RawResponseJson;

                var entryDto = await _entryService.AddEntryAsync(HardcodedUserId, date, dto);
                result.CreatedEntries.Add(entryDto);
            }

            return Ok(result);
        }
    }
}