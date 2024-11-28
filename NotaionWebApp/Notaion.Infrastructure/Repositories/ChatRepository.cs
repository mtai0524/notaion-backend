using Notaion.Application.Common.Helpers;
using Notaion.Application.Services;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatModelTrainer _chatModelTrainer;
        private readonly EncryptionService _encryptionService;
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _chatModelTrainer = new ChatModelTrainer();
            _encryptionService = new EncryptionService();
        }

        public async Task<Chat> AddChatbotAsync(Chat chat)
        {
            var botResponse = await GetChatbotResponseAsync(chat.Content);

            var encryptChatbot = _encryptionService.Encrypt(botResponse);

            var botChat = new Chat
            {
                Content = encryptChatbot,
                UserId = "2071b4d2-734d-479b-8b64-ba4e81c30231",
                UserName = "Chatbot",
                SentDate = DateTimeHelper.GetVietnamTime(),
                Hide = false
            };

            await _context.Chat.AddAsync(botChat);

            return botChat;
        }


        public async Task<string> GetChatbotResponseAsync(string userMessage)
        {
            if (userMessage.StartsWith("/bot", StringComparison.OrdinalIgnoreCase))
            {
                userMessage = userMessage.Length > 5 ? userMessage.Substring(5).Trim() : string.Empty;
            }

            if (_chatModelTrainer == null)
            {
                throw new InvalidOperationException("ChatModelTrainer is not initialized.");
            }
            var response = await _chatModelTrainer.PredictResponseAsync(userMessage);

            return response;
        }
    }
}
