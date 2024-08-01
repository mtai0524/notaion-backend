using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
using Notaion.Entities;
using Notaion.Models;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PageController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePage(CreatePage createPage)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var page = new Page
            {
                Id = Guid.NewGuid().ToString(),
                Title = createPage.Title,
                Content = createPage.Content,
                UserId = createPage.UserId,
                CreatedAt = vietnamTime,
                UpdatedAt = vietnamTime,
                Public = createPage.Public
            };

            _context.Page.Add(page);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPageById), new { id = page.Id }, page);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPageById(string id)
        {
            var page = await _context.Page.FindAsync(id);

            if (page == null)
            {
                return NotFound();
            }

            return Ok(page);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPagesByUserId(string userId)
        {
            var pages = await _context.Page.Where(p => p.UserId == userId).ToListAsync();

            if (!pages.Any())
            {
                return NotFound();
            }

            return Ok(pages);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePageContent(string id, [FromBody] UpdatePageContentDto updatePageContentDto)
        {
            var page = await _context.Page.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }

            page.Content = updatePageContentDto.Content;
            page.Title = updatePageContentDto.Title;

            _context.Page.Update(page);
            await _context.SaveChangesAsync();

            return NoContent();
        }
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeletePage (string id)
            {
                var page = await _context.Page.Where(x=> x.Id == id).FirstOrDefaultAsync();
                if (page == null)
                {
                    return NotFound();
                }
                _context.Page.Remove(page);
                await _context.SaveChangesAsync();
                return Ok();
            }

    }
}
