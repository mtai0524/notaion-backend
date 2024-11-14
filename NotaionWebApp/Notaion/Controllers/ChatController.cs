using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Notaion.Context;
using Notaion.Entities;
using Notaion.Models;
using Notaion.Services;
using Notaion.Hubs;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Notaion.Domain.Models;
using Notaion.Domain.Entities;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        [Authorize]
        [HttpGet("get-chats")]
        public IActionResult GetChats()
        {
            var chats = _context.Chat
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.SentDate,
                    UserName = c.User.UserName,
                    c.Hide
                })
                .OrderBy(c => c.SentDate)
                .Where(x => x.Hide == false)
                .ToList();

            return Ok(chats);
        }

        [HttpGet("get-chats-hidden")]
        public IActionResult GetChatsHidden()
        {
            var chats = _context.Chat
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.SentDate,
                    UserName = c.User.UserName,
                    c.Hide
                })
                .OrderBy(c => c.SentDate)
                .Where(x => x.Hide == true)
                .ToList();

            return Ok(chats);
        }

        [HttpPost("add-chat")]
        public async Task<IActionResult> AddChat([FromBody] ChatViewModel chatViewModel)
        {
            if (chatViewModel == null || string.IsNullOrEmpty(chatViewModel.Content))
            {
                return BadRequest("Invalid chat message.");
            }

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var userName = !string.IsNullOrEmpty(chatViewModel.UserName)
                ? chatViewModel.UserName
                : "mèo con ẩn danh";

            var chat = new Chat
            {
                Id = Guid.NewGuid().ToString(),
                Content = chatViewModel.Content,
                SentDate = vietnamTime,
                UserId = chatViewModel.UserId ?? "anonymous",
                UserName = userName
            };

            _context.Chat.Add(chat);
            await _context.SaveChangesAsync();
       
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", userName, chat.Content);

            return Ok(chat);
        }

        [HttpDelete("delete-all-chats")]
        public async Task<IActionResult> DeleteAllChats()
        {
            var listChats = await _context.Chat.ToListAsync();

            foreach (var chat in listChats)
            {
                chat.Hide = true;
            }
            _context.UpdateRange(listChats);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("delete-me-chats/{id}")]
        public async Task<IActionResult> DeleteMeChats(string id)
        {
            var listChats = await _context.Chat.Where(x => x.User.Id == id).ToListAsync();
            if (listChats == null)
            {
                return BadRequest();
            }
            foreach (var chat in listChats)
            {
                chat.Hide = true;
            }
            _context.UpdateRange(listChats);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}

