using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Infrastructure.Context;
using Notaion.Hubs;

namespace Notaion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-all-noti")]
        public async Task<IActionResult> GetAllNoti()
        {
            var listNotis = await _context.Notification.ToListAsync();
            return Ok(listNotis);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNotiList()
        {
            var listNotis = await _context.Notification.ToListAsync();
            _context.RemoveRange(listNotis);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("clear-by-receiver/{receiverId}")]
        public async Task<IActionResult> ClearNotificationsByReceiverId(string receiverId)
        {
            try
            {
                var notificationsToRemove = _context.Notification.Where(n => n.ReceiverId == receiverId);

                if (!notificationsToRemove.Any())
                {
                    return NotFound("No notifications found for the user.");
                }

                _context.Notification.RemoveRange(notificationsToRemove);
                await _context.SaveChangesAsync();

                return Ok("All notifications cleared successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpPut("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var notification = await _context.Notification.FindAsync(notificationId);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            _context.Update(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("get-noti-by-recvid/{id}")]
        public async Task<IActionResult> GetNotiByRecvId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Recipient ID cannot be null or empty.");
            }

            var notiLists = await _context.Notification
                .Where(x => x.ReceiverId == id)
                .ToListAsync();

            return Ok(notiLists);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification == null)
            {
                return NotFound(new { Message = "Notification not found" });
            }

            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Notification deleted successfully" });
        }
    }
}
