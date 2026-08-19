using EzFit.DTOs.Ai;
using EzFit.DTOs.Requests;
using EzFit.DTOs.Responses;
using EzFit.Entities;
using EzFit.Options;
using EzFit.Services.Interfaces;
using EzFit.Services.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("log")]
    public class LogController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAiService _aiService;
        private readonly IEntryService _entryService;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly UploadsOptions _uploadsOptions;

        public LogController(
            IImageService imageService,
            IFileStorageService fileStorageService,
            IAiService aiService,
            IEntryService entryService,
            ICurrentUserProvider currentUserProvider,
            IOptions<UploadsOptions> uploadsOptions)
        {
            _imageService = imageService;
            _fileStorageService = fileStorageService;
            _aiService = aiService;
            _entryService = entryService;
            _currentUserProvider = currentUserProvider;
            _uploadsOptions = uploadsOptions.Value;
        }

        // POST api/log?date=2026-08-18  (form-data: message=<text>, images=<file(s)>)
        [HttpPost]
        public async Task<ActionResult<LogResultDto>> Log(
            [FromForm] string? message,
            [FromForm] List<IFormFile>? images,
            CancellationToken cancellationToken)
        {
            if (images is not null && images.Count > 0)
            {
                if (images.Count > _uploadsOptions.MaxFileCount)
                {
                    return BadRequest($"No more than {_uploadsOptions.MaxFileCount} images may be uploaded per request.");
                }

                var oversizedImage = images.FirstOrDefault(image => image.Length > _uploadsOptions.MaxFileSizeBytes);
                if (oversizedImage is not null)
                {
                    return BadRequest($"'{oversizedImage.FileName}' exceeds the maximum file size of {_uploadsOptions.MaxFileSizeBytes} bytes.");
                }

                // Screenshots alone are ambiguous without user-provided context — enforced
                // server-side since a frontend-only check is trivially bypassed.
                if (string.IsNullOrWhiteSpace(message))
                {
                    return BadRequest("Add a short description alongside your screenshots so the AI knows what to extract.");
                }
            }

            var imageBytesForAi = new List<byte[]>();
            var savedBaseNames = new List<string>();

            if (images is not null)
            {
                var totalTiles = 0;

                foreach (var image in images)
                {
                    var tiles = await _imageService.ProcessAsync(image.OpenReadStream(), cancellationToken);

                    // Tallied across all files in the request, not per file — check before
                    // decoding the next image so an over-limit request stops early instead
                    // of paying for every file's decode before rejecting.
                    totalTiles += tiles.Count;
                    if (totalTiles > _uploadsOptions.MaxTilesPerRequest)
                    {
                        foreach (var tile in tiles)
                        {
                            tile.Dispose();
                        }

                        return BadRequest(
                            $"These images would require too many tiles to process ({totalTiles} > {_uploadsOptions.MaxTilesPerRequest}). " +
                            "Split the upload into fewer or shorter screenshots.");
                    }

                    // Encode a copy for the AI call before FileStorageService disposes the tiles
                    foreach (var tile in tiles)
                    {
                        using var ms = new MemoryStream();
                        await tile.SaveAsync(ms, new WebpEncoder(), cancellationToken);
                        imageBytesForAi.Add(ms.ToArray());
                    }

                    var baseName = _fileStorageService.GenerateBaseName(_currentUserProvider.UserId);
                    await _fileStorageService.SaveAsync(baseName, tiles, cancellationToken); // disposes tiles internally

                    savedBaseNames.Add(baseName);
                }
            }

            var aiResponse = await _aiService.ExtractAsync(message, imageBytesForAi, cancellationToken);
            var imagePath = savedBaseNames.Count > 0 ? string.Join(",", savedBaseNames) : null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
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

                // Each entry may resolve to a different day if the AI recognized a
                // date reference; entries without one default to today.
                var targetDate = dto.OccurredAt.HasValue
                    ? DateOnly.FromDateTime(dto.OccurredAt.Value)
                    : today;

                var entryDto = await _entryService.AddEntryAsync(_currentUserProvider.UserId, targetDate, dto, cancellationToken);
                result.CreatedEntries.Add(entryDto);
            }

            return Ok(result);
        }
    }
}
