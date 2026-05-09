using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces.Services;
using Notaion.Application.Options;
using Notaion.Hubs;
using Notaion.Infrastructure.Context;
using Notaion.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService chatService;
        private readonly IAIService _aiService;
        private readonly IEncryptionService _encryptionService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IChatService chatService, IAIService aiService, IEncryptionService encryptionService, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            this.chatService = chatService;
            _hubContext = hubContext;
            _aiService = aiService;
            _encryptionService = encryptionService;
            _scopeFactory = scopeFactory;
        }

            [HttpPost("train")]
            public async Task<IActionResult> TrainChatbotModel()
            {
                return Ok("Mô hình hiện đã chuyển sang sử dụng LLM (Gemini), không cần huấn luyện thủ công nữa.");
            }

        // Dự đoán câu trả lời từ câu hỏi của người dùng
        [HttpPost("predict")]
        public async Task<IActionResult> PredictResponse([FromBody] string userMessage)
        {
            try
            {
                var response = await _aiService.GetAIResponseAsync(userMessage);

                var cleanedResponse = response.Trim();

                return Ok(new { Response = cleanedResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[Authorize]
        [HttpGet("get-chats")]
        [SwaggerOperation(Summary = "Get list of chats")]
        public async Task<IActionResult> GetChats([FromQuery] QueryOptions options,
                                           [FromQuery, SwaggerIgnore] bool decrypt = false)
        {
            var result = await this.chatService.GetChatsAsync(options, decrypt);
            return Ok(result);
        }

        [HttpGet("get-chats-hidden")]
        public async Task<IActionResult> GetChatsHidden([FromQuery] bool decrypt = false)
        {
            return Ok(await this.chatService.GetChatsHiddenAsync());
        }

        [HttpPost("add-chat")]
        public async Task<IActionResult> AddChat([FromBody] CreateChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Content))
            {
                return BadRequest("Invalid chat message.");
            }

            try
            {
                Console.WriteLine($"[AddChat] Received Content: {chatDto.Content}");

                var createdChat = await this.chatService.CreateChatAsync(chatDto);

                if (string.IsNullOrEmpty(createdChat.UserName))
                {
                    createdChat.UserName = "mèo con ẩn danh";
                }

                var decryptedContent = _encryptionService.Decrypt(createdChat.Content);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChat.UserName, decryptedContent);

                if (decryptedContent.Contains("/bot"))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var scopedChatService = scope.ServiceProvider.GetRequiredService<IChatService>();
                                var scopedEncryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
                                
                                // Tạo một DTO mới cho bot
                                var botChatDto = new CreateChatDto 
                                { 
                                    Content = decryptedContent, 
                                    UserId = chatDto.UserId,
                                    UserName = chatDto.UserName
                                };

                                var createdChatbot = await scopedChatService.CreateChatbotAsync(botChatDto);
                                var decryptedChatbotContent = scopedEncryptionService.Decrypt(createdChatbot.Content);
                                
                                // Gửi qua SignalR bằng HubContext chính của Controller
                                await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChatbot.UserName, decryptedChatbotContent);
                                Console.WriteLine($"[AI-Success] Sent SignalR message: {createdChatbot.UserName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[AI-Error] {ex.Message}");
                        }
                    });
                }


                createdChat.Content = decryptedContent;

                return Ok(createdChat);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"[Error] FormatException: {ex.Message}");
                return BadRequest($"Invalid Base64 content: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Exception: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpDelete("delete-all-chats")]
        public async Task<IActionResult> DeleteAllChats()
        {
            try
            {
                var updateRecords = await chatService.HideChatAllAsync();
                return Ok(new { Message = "All chats msg is hidden successfully", UpdateRecords = updateRecords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "error when hide chats", Error = ex.Message });
            }
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
        [HttpPost("update-ai-memory")]
        public async Task<IActionResult> UpdateAiMemory([FromBody] CreateChatDto memoryDto)
        {
            if (memoryDto.UserName != "minhtai")
            {
                return Forbid("Chỉ người dùng minhtai mới có quyền ghi vào bộ nhớ AI.");
            }

            try
            {
                string memoryPath = Path.Combine(Directory.GetCurrentDirectory(), "ai_memory.txt");
                
                // Ghi thêm nội dung vào cuối file (Append) kèm theo xuống dòng
                await System.IO.File.AppendAllTextAsync(memoryPath, memoryDto.Content + Environment.NewLine);
                
                return Ok(new { message = "Bộ nhớ AI đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi ghi file bộ nhớ: {ex.Message}");
            }
        }
    }
}

