using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Entities;
using Notaion.Infrastructure.Context;

namespace Notaion.Controllers
{
    // Per-account private 1-on-1 AI assistant. Each user has their own thread,
    // stored encrypted at rest and never shared with other users or the public
    // chat room.
    [Route("api/[controller]")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        // How many recent turns of the user's own thread to feed the model as context.
        private const int MaxHistoryTurns = 16;

        private readonly ApplicationDbContext _context;
        private readonly IAIService _aiService;
        private readonly IEncryptionService _encryptionService;

        public AiChatController(
            ApplicationDbContext context,
            IAIService aiService,
            IEncryptionService encryptionService)
        {
            _context = context;
            _aiService = aiService;
            _encryptionService = encryptionService;
        }

        // Send a message to the user's personal assistant and get a reply.
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] AiChatSendDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("UserId and Content are required.");
            }

            var userId = dto.UserId;

            // Persist the user's message (encrypted).
            var userMessage = new AiChat
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = "user",
                Content = _encryptionService.Encrypt(dto.Content.Trim()),
                SentDate = DateTime.UtcNow
            };
            _context.AiChats.Add(userMessage);
            await _context.SaveChangesAsync();

            // Build context from THIS user's recent turns only.
            var recent = await _context.AiChats
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.SentDate)
                .Take(MaxHistoryTurns)
                .ToListAsync();
            recent.Reverse(); // back to chronological order

            var conversation = recent
                .Select(m => new ChatTurn(
                    m.Role == "assistant" ? ChatRole.Assistant : ChatRole.User,
                    SafeDecrypt(m.Content)))
                .ToList();

            string reply;
            try
            {
                reply = await _aiService.GetAIResponseAsync(conversation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI assistant error: {ex.Message}");
                reply = "Xin lỗi, trợ lý AI hiện không phản hồi được. Vui lòng thử lại.";
            }

            // Persist the assistant reply (encrypted).
            var botMessage = new AiChat
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = "assistant",
                Content = _encryptionService.Encrypt(reply),
                SentDate = DateTime.UtcNow
            };
            _context.AiChats.Add(botMessage);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                role = "assistant",
                content = reply,
                sentDate = botMessage.SentDate
            });
        }

        // Load the full assistant conversation for a user (decrypted).
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> History(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("UserId is required.");
            }

            var messages = await _context.AiChats
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.SentDate)
                .ToListAsync();

            var result = messages.Select(m => new
            {
                role = m.Role,
                content = SafeDecrypt(m.Content),
                sentDate = m.SentDate
            });

            return Ok(result);
        }

        // Wipe the user's assistant conversation.
        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> Clear(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("UserId is required.");
            }

            var messages = _context.AiChats.Where(m => m.UserId == userId);
            _context.AiChats.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(new { cleared = true });
        }

        private string SafeDecrypt(string? content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            try
            {
                return _encryptionService.Decrypt(content);
            }
            catch (CryptographicException)
            {
                return content;
            }
            catch (FormatException)
            {
                return content;
            }
        }
    }
}
