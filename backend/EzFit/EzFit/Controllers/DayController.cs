using EzFit.DTOs.Responses;
using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DayController : ControllerBase
    {
        private readonly IDayService _dayService;

        // Без автентикация все още — hardcoded, докато не стигнем до Etap 4
        private const int HardcodedUserId = 1;

        public DayController(IDayService dayService)
        {
            _dayService = dayService;
        }

        // GET api/day?date=2026-08-09
        [HttpGet]
        public async Task<ActionResult<DaySummaryDto>> GetDaySummary([FromQuery] DateOnly date)
        {
            var summary = await _dayService.GetDaySummaryAsync(HardcodedUserId, date);
            return Ok(summary);
        }
    }
}