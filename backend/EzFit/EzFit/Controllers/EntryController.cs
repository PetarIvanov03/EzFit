using EzFit.DTOs.Requests;
using EzFit.DTOs.Responses;
using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntryController : ControllerBase
    {
        private readonly IEntryService _entryService;

        private const int HardcodedUserId = 1;

        public EntryController(IEntryService entryService)
        {
            _entryService = entryService;
        }

        // POST api/entry?date=2026-08-09
        [HttpPost]
        public async Task<ActionResult<EntryDto>> AddEntry([FromQuery] DateOnly date, [FromBody] CreateEntryDto dto)
        {
            try
            {
                var result = await _entryService.AddEntryAsync(HardcodedUserId, date, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}