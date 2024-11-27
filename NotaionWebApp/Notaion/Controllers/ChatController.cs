using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces.Services;
using Notaion.Hubs;
using Notaion.Infrastructure.Context;
using Notaion.Infrastructure.Repositories;

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

        //[HttpGet("test-genaric-repo")]
        //public async Task<IActionResult> GetChatWithGenaricRepo()
        //{
        //    var chats = await chatService.GetChatsAsync();
        //    return Ok(chats);
        //}

        //[Authorize]
        [HttpGet("get-chats")]
        public async Task<IActionResult> GetChats()
        {
            return Ok(await this.chatService.GetChatsAsync());
        }

        [HttpGet("get-chats-hidden")]
        public async Task<IActionResult> GetChatsHidden()
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
                var createdChat = await this.chatService.CreateChatAsync(chatDto);

                if (string.IsNullOrEmpty(createdChat.UserName))
                {
                    createdChat.UserName = "mèo con ẩn danh";
                }

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChat.UserName, _encryptionService.Decrypt(createdChat.Content));


                if (chatDto.Content.Contains("/bot"))
                {
                    var createdChatbot = await this.chatService.CreateChatbotAsync(chatDto);

                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChatbot.UserName, _encryptionService.Decrypt(createdChat.Content));
                }

                return Ok(createdChat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
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

