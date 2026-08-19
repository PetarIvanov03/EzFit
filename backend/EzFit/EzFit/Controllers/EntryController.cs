using EzFit.DTOs.Requests;
using EzFit.DTOs.Responses;
using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    public class EntryController : ControllerBase
    {
        private readonly IEntryService _entryService;
        private readonly ICurrentUserProvider _currentUserProvider;

        public EntryController(IEntryService entryService, ICurrentUserProvider currentUserProvider)
        {
            _entryService = entryService;
            _currentUserProvider = currentUserProvider;
        }

        // POST api/entry?date=2026-08-09
        [HttpPost]
        public async Task<ActionResult<EntryDto>> AddEntry([FromQuery] DateOnly date, [FromBody] CreateEntryDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _entryService.AddEntryAsync(_currentUserProvider.UserId, date, dto, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
