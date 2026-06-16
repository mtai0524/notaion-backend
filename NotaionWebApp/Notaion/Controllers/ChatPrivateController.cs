using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.Interfaces.Services;
using Notaion.Infrastructure.Context;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;
using Notaion.Hubs;
using Notaion.Models;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Notaion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatPrivateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IEncryptionService _encryptionService;

        public ChatPrivateController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IEncryptionService encryptionService)
        {
            _context = context;
            _hubContext = hubContext;
            _encryptionService = encryptionService;
        }

        private string SafeDecrypt(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            try
            {
                return _encryptionService.Decrypt(content);
            }
            catch (CryptographicException)
            {
                return "[Unable to decrypt message]";
            }
            catch (FormatException)
            {
                // Not Base64 — treat as legacy plaintext
                return content;
            }
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
                Content = SafeDecrypt(chat.Content),
                SentDate = chat.SentDate.HasValue ? chat.SentDate.Value : DateTime.MinValue,
                Sender = chat.Sender,
                Receiver = chat.Receiver,
                CurrentUserName = chat.Sender == currentUserId ? currentUser.UserName : friendUser.UserName,
                FriendUserName = chat.Receiver == friendId ? friendUser.UserName : currentUser.UserName
            }).ToList();

            return Ok(chatsWithUsers);
        }


        public class ChatSearchResult
        {
            public string FriendId { get; set; }
            public string FriendUserName { get; set; }
            public string FriendAvatar { get; set; }
            public string Content { get; set; }
            public string Snippet { get; set; }
            public DateTime SentDate { get; set; }
            public bool FromMe { get; set; }
        }

        // Full-text search across ALL of the current user's private conversations.
        // Messages are encrypted at rest, so we must decrypt in-memory before matching —
        // a SQL LIKE on Content would only ever match ciphertext.
        [HttpGet("search/{currentUserId}")]
        public async Task<IActionResult> SearchChats(string currentUserId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Ok(new List<ChatSearchResult>());
            }

            var chats = await _context.ChatPrivate
                .Where(c => c.Sender == currentUserId || c.Receiver == currentUserId)
                .OrderByDescending(c => c.SentDate)
                .ToListAsync();

            var matches = new List<ChatSearchResult>();
            foreach (var chat in chats)
            {
                var content = SafeDecrypt(chat.Content);
                if (string.IsNullOrEmpty(content)) continue;

                var idx = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) continue;

                // Build a short snippet centred on the match.
                var start = Math.Max(0, idx - 30);
                var len = Math.Min(content.Length - start, keyword.Length + 60);
                var snippet = (start > 0 ? "…" : "") + content.Substring(start, len).Trim()
                              + (start + len < content.Length ? "…" : "");

                matches.Add(new ChatSearchResult
                {
                    FriendId = chat.Sender == currentUserId ? chat.Receiver : chat.Sender,
                    Content = content,
                    Snippet = snippet,
                    SentDate = chat.SentDate ?? DateTime.MinValue,
                    FromMe = chat.Sender == currentUserId,
                });
            }

            // Attach friend display info (username + avatar) in one round-trip.
            var friendIds = matches.Select(m => m.FriendId).Distinct().ToList();
            var users = await _context.User
                .Where(u => friendIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var m in matches)
            {
                if (m.FriendId != null && users.TryGetValue(m.FriendId, out var u))
                {
                    m.FriendUserName = u.UserName;
                    m.FriendAvatar = u.Avatar;
                }
            }

            return Ok(matches);
        }

        [HttpPost("add-chat-private")]
        public async Task<IActionResult> AddChat([FromBody] ChatPrivateViewModel chatViewModel)
        {
            if (chatViewModel == null || string.IsNullOrEmpty(chatViewModel.Content))
            {
                return BadRequest("Invalid chat message.");
            }

            var plainContent = chatViewModel.Content;
            var encryptedContent = _encryptionService.Encrypt(plainContent);

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var chatPrivate = new ChatPrivate
            {
                Id = Guid.NewGuid().ToString(),
                Content = encryptedContent,
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

            // Broadcast plaintext so receivers don't need an encryption key.
            await _hubContext.Clients.All.SendAsync(
                "ReceiveMessagePrivate", chatViewModel.SenderId, chatViewModel.ReceiverId, plainContent, currentUser.UserName, friendUser.UserName);

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
