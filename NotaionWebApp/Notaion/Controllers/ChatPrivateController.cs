using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;
using Notaion.Entities;
using Notaion.Hubs;
using Notaion.Models;
using System.Security.Claims;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatPrivateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatPrivateController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public class PrivateChatWithUsers
        {
            public string Content { get; set; }
            public DateTime SentDate { get; set; }
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string CurrentUserName { get; set; }
            public string FriendUserName { get; set; }
        }

        [HttpGet("get-chats-private/{currentUserId}/{friendId}")]
        public async Task<IActionResult> GetChatsPrivate(string currentUserId, string friendId)
        {
            var currentUser = await _context.User.FirstOrDefaultAsync(x => x.Id == currentUserId);
            var friendUser = await _context.User.FirstOrDefaultAsync(x => x.Id == friendId);

            if (currentUser == null || friendUser == null)
            {
                return NotFound("One or both users were not found.");
            }

            var chats = await _context.ChatPrivate
                .Where(chat => (chat.Sender == currentUserId && chat.Receiver == friendId) ||
                               (chat.Sender == friendId && chat.Receiver == currentUserId))
                .OrderBy(chat => chat.SentDate)
                .ToListAsync();

            var chatsWithUsers = chats.Select(chat => new PrivateChatWithUsers
            {
                Content = chat.Content,
                SentDate = chat.SentDate.HasValue ? chat.SentDate.Value : DateTime.MinValue,
                Sender = chat.Sender,
                Receiver = chat.Receiver,
                CurrentUserName = chat.Sender == currentUserId ? currentUser.UserName : friendUser.UserName,
                FriendUserName = chat.Receiver == friendId ? friendUser.UserName : currentUser.UserName
            }).ToList();

            return Ok(chatsWithUsers);
        }


        [HttpPost("add-chat-private")]
        public async Task<IActionResult> AddChat([FromBody] ChatPrivateViewModel chatViewModel)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var chatPrivate = new ChatPrivate
            {
                Id = Guid.NewGuid().ToString(),
                Content = chatViewModel.Content,
                SentDate = vietnamTime,
                Sender = chatViewModel.SenderId,
                Receiver = chatViewModel.ReceiverId,
                Hide = false,
                IsNew = true,
            };

            var currentUser = await _context.User.FirstOrDefaultAsync(x => x.Id == chatViewModel.SenderId);
            var friendUser = await _context.User.FirstOrDefaultAsync(x => x.Id == chatViewModel.ReceiverId);


            _context.ChatPrivate.Add(chatPrivate);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "ReceiveMessagePrivate", chatViewModel.SenderId, chatViewModel.ReceiverId, chatViewModel.Content, currentUser.UserName, friendUser.UserName);

            return Ok();
        }

        [HttpGet("new-messages/{friendId}/{currentUserId}")]
        public async Task<IActionResult> GetNewMessageCount(string friendId, string currentUserId)
        {
            var newMessageCount = await _context.ChatPrivate
                .CountAsync(m => m.Sender == friendId && m.Receiver == currentUserId && m.IsNew);

            return Ok(newMessageCount);
        }


        [HttpPost("reset-new-messages/{friendId}/{currentUserId}")]
        public async Task<IActionResult> ResetNewMessages(string friendId, string currentUserId)
        {
            var messagesToReset = await _context.ChatPrivate
                .Where(m => m.Receiver == currentUserId && m.Sender == friendId && m.IsNew)
                .ToListAsync();

            if (!messagesToReset.Any())
            {
                return NotFound("No new messages to reset for this friend.");
            }

            foreach (var message in messagesToReset)
            {
                message.IsNew = false;
                _context.ChatPrivate.Update(message); // Cập nhật từng tin nhắn
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return NoContent();
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteAllChatPrivate()
        {
            var listChatPrivate = await _context.ChatPrivate.ToListAsync();
            _context.RemoveRange(listChatPrivate);
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}
