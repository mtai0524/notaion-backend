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
        private readonly ChatModelTrainer _chatModelTrainer;
        private readonly IEncryptionService _encryptionService;

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IChatService chatService, ChatModelTrainer chatModelTrainer, IEncryptionService encryptionService)
        {
            _context = context;
            this.chatService = chatService;
            _hubContext = hubContext;
            _chatModelTrainer = chatModelTrainer;
            _encryptionService = encryptionService;
        }

        [HttpPost("train")]
        public async Task<IActionResult> TrainChatbotModel()
        {
            try
            {
                // Sử dụng đường dẫn cố định đến file responses.csv
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "responses.csv");

                // Kiểm tra xem file có tồn tại không
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"File không tồn tại tại đường dẫn: {filePath}");
                }
                var modelTrainer = new ChatModelTrainer();
                // Huấn luyện mô hình từ file
                await modelTrainer.TrainModelFromCsvAsync(filePath);
                return Ok("Mô hình đã được huấn luyện thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi khi huấn luyện mô hình: {ex.Message}");
            }
        }

        // Dự đoán câu trả lời từ câu hỏi của người dùng
        [HttpPost("predict")]
        public async Task<IActionResult> PredictResponse([FromBody] string userMessage)
        {
            try
            {
                var response = await _chatModelTrainer.PredictResponseAsync(userMessage);

                var cleanedResponse = response.Replace("\"", "").Trim();

                return Ok(new { Response = cleanedResponse });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi trong quá trình dự đoán: {ex.Message}");
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
                    Console.WriteLine($"[ChatBot] Triggered for content: {decryptedContent}");
                    var createdChatbot = await this.chatService.CreateChatbotAsync(chatDto);
                    var decryptedChatbotContent = _encryptionService.Decrypt(createdChatbot.Content);

                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChatbot.UserName, decryptedChatbotContent);
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
    }
}

