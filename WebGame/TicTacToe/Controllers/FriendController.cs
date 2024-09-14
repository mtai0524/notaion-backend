using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
using Notaion.Entities;
using Notaion.Hubs;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public FriendController(IHubContext<ChatHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNotiList()
        {
            var listNotis = await _context.Notification.ToListAsync();
            _context.RemoveRange(listNotis);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("send-friend-request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            var recvUser = await _context.User
                .Where(x => x.UserName == request.RecipientId)
                .FirstOrDefaultAsync();

            if (recvUser == null)
            {
                return NotFound("Recipient not found");
            }

            var notification = new Notification
            {
                SenderId = request.RequesterId,
                SenderName = request.RequesterName,
                ReceiverId = recvUser.Id,
                ReceiverName = recvUser.UserName,
                Content = $"{request.RequesterName} has sent you a friend request.",
                SenderAvatar = request.Avatar,
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveFriendRequest", request.RequesterId, recvUser.Id, request.RequesterName);

            return Ok();
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



        public class FriendRequestDto
        {
            public string RequesterId { get; set; }
            public string RequesterName { get; set; }
            public string RecipientId { get; set; }
            public string Avatar { get; set; }
        }
    }
}
