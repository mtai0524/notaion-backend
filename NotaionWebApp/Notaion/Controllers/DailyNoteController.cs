using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Entities;
using Notaion.Hubs;
using Notaion.Infrastructure.Context;
using System.Security.Claims;

namespace Notaion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyNoteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DailyNoteHub> _hubContext;

        public DailyNoteController(ApplicationDbContext context, IHubContext<DailyNoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<IEnumerable<DailyNote>>> GetNotesByDate(string date, [FromQuery] bool includeDeleted = false)
        {
            var userId = GetUserId();
            var query = _context.DailyNotes.Where(n => n.Date == date && n.UserId == userId);
            
            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }
            
            return await query.ToListAsync();
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<DailyNote>>> GetAllNotes([FromQuery] bool includeDeleted = false)
        {
            var userId = GetUserId();
            var query = _context.DailyNotes.Where(n => n.UserId == userId);

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DailyNote>> UpsertNote(DailyNote note)
        {
            var userId = GetUserId();
            note.UserId = userId;

            var existingNote = await _context.DailyNotes.FirstOrDefaultAsync(n => n.Id == note.Id);
            bool isNew = existingNote == null;

            if (existingNote != null)
            {
                _context.Entry(existingNote).CurrentValues.SetValues(note);
                _context.Entry(existingNote).State = EntityState.Modified;
            }
            else
            {
                _context.DailyNotes.Add(note);
            }

            await _context.SaveChangesAsync();

            // Notify clients via SignalR
            await _hubContext.Clients.Group(userId).SendAsync(isNew ? "NoteCreated" : "NoteUpdated", note);

            return Ok(note);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUpdateNotes(IEnumerable<DailyNote> notes)
        {
            var userId = GetUserId();
            foreach (var note in notes)
            {
                note.UserId = userId;
                var existingNote = await _context.DailyNotes.FirstOrDefaultAsync(n => n.Id == note.Id);
                if (existingNote != null)
                {
                    _context.Entry(existingNote).CurrentValues.SetValues(note);
                    _context.Entry(existingNote).State = EntityState.Modified;
                }
                else
                {
                    _context.DailyNotes.Add(note);
                }
            }

            await _context.SaveChangesAsync();

            // Notify clients via SignalR about bulk update
            await _hubContext.Clients.Group(userId).SendAsync("NotesBulkUpdated", notes);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var userId = GetUserId();
            var note = await _context.DailyNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null) return NotFound();

            note.IsDeleted = true;
            _context.Entry(note).Property(x => x.IsDeleted).IsModified = true;

            await _context.SaveChangesAsync();

            // Notify clients via SignalR
            await _hubContext.Clients.Group(userId).SendAsync("NoteDeleted", id);

            return Ok();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<DailyNote>> RestoreNote(string id)
        {
            var userId = GetUserId();
            var note = await _context.DailyNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null) return NotFound();

            note.IsDeleted = false;
            _context.Entry(note).Property(x => x.IsDeleted).IsModified = true;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(userId).SendAsync("NoteRestored", note);

            return Ok(note);
        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDeleteNote(string id)
        {
            var userId = GetUserId();
            var note = await _context.DailyNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null) return NotFound();

            _context.DailyNotes.Remove(note);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(userId).SendAsync("NotePurged", id);

            return Ok();
        }
    }
}
