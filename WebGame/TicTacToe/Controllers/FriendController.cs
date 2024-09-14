using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
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
        [HttpPost("send-friend-request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            var recvUser = await _context.User
                .Where(x => x.UserName == request.RecipientId)
                .FirstOrDefaultAsync();
            var userName = recvUser.UserName;
            if (recvUser == null || userName == null)
            {
                return NotFound("Recipient not found");
            }

           await _hubContext.Clients.All.SendAsync("ReceiveFriendRequest", request.RequesterId, request.RequesterName);

            return Ok();
        }
    }
    public class FriendRequestDto
    {
        public string RequesterId { get; set; }
        public string RequesterName { get; set; }
        public string RecipientId { get; set; }
    }
}
