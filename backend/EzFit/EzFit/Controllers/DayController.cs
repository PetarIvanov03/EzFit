using EzFit.DTOs.Responses;
using EzFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EzFit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    public class DayController : ControllerBase
    {
        private readonly IDayService _dayService;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DayController(IDayService dayService, ICurrentUserProvider currentUserProvider)
        {
            _dayService = dayService;
            _currentUserProvider = currentUserProvider;
        }

        // GET api/day?date=2026-08-09
        [HttpGet]
        public async Task<ActionResult<DaySummaryDto>> GetDaySummary([FromQuery] DateOnly date, CancellationToken cancellationToken)
        {
            var summary = await _dayService.GetDaySummaryAsync(_currentUserProvider.UserId, date, cancellationToken);
            return Ok(summary);
        }

        // GET api/day/list?count=7
        [HttpGet("list")]
        public async Task<ActionResult<List<DaySummaryDto>>> GetRecentDays([FromQuery] int count, CancellationToken cancellationToken)
        {
            if (count <= 0) count = 7;
            if (count > 30) count = 30;

            var summaries = await _dayService.GetRecentDaySummariesAsync(_currentUserProvider.UserId, count, cancellationToken);
            return Ok(summaries);
        }
    }
}
