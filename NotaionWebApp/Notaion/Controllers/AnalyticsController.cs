using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Entities;
using Notaion.Infrastructure.Context;

namespace Notaion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackVisit([FromBody] TrackVisitDto dto)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var visit = new PageVisit
            {
                Path = dto.Path,
                IsLocalhost = dto.IsLocalhost,
                Timestamp = vietnamTime
            };

            _context.PageVisits.Add(visit);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmapData()
        {
            var data = await _context.PageVisits
                .GroupBy(v => new { Date = v.Timestamp.Date, v.IsLocalhost })
                .Select(g => new
                {
                    Date = g.Key.Date.ToString("yyyy-MM-dd"),
                    IsLocalhost = g.Key.IsLocalhost,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }
    }

    public class TrackVisitDto
    {
        public string? Path { get; set; }
        public bool IsLocalhost { get; set; }
    }
}
