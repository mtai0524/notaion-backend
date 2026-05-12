using Microsoft.EntityFrameworkCore;
using Notaion.Application.Common.Helpers;
using Notaion.Application.Interfaces.Services;
using Notaion.Application.Services;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        private const int HistoryTurns = 12;
        private const string BotUserName = "Chatbot";

        private readonly ApplicationDbContext _context;
        private readonly IAIService _aiService;
        private readonly IEncryptionService _encryptionService;

        public ChatRepository(ApplicationDbContext context, IAIService aiService, IEncryptionService encryptionService) : base(context)
        {
            _context = context;
            _aiService = aiService;
            _encryptionService = encryptionService;
        }

        public async Task<Chat> AddChatbotAsync(Chat chat)
        {
            var botResponse = await GetChatbotResponseAsync(chat.Content);

            var encryptChatbot = _encryptionService.Encrypt(botResponse);

            var botChat = new Chat
            {
                Content = encryptChatbot,
                UserId = "2071b4d2-734d-479b-8b64-ba4e81c30231",
                UserName = BotUserName,
                SentDate = DateTimeHelper.GetVietnamTime(),
                Hide = false
            };

            await _context.Chat.AddAsync(botChat);

            return botChat;
        }


        public async Task<string> GetChatbotResponseAsync(string userMessage)
        {
            if (userMessage != null && userMessage.StartsWith("/bot", StringComparison.OrdinalIgnoreCase))
            {
                userMessage = userMessage.Length > 5 ? userMessage.Substring(5).Trim() : string.Empty;
            }

            if (_aiService == null)
            {
                throw new InvalidOperationException("AI Service is not initialized.");
            }

            var conversation = await BuildConversationHistoryAsync(userMessage ?? string.Empty);
            return await _aiService.GetAIResponseAsync(conversation);
        }

        private async Task<IReadOnlyList<ChatTurn>> BuildConversationHistoryAsync(string currentUserMessage)
        {
            var recent = await _context.Chat
                .AsNoTracking()
                .Where(c => c.Hide == false)
                .OrderByDescending(c => c.SentDate)
                .Take(HistoryTurns)
                .ToListAsync();

            recent.Reverse();

            var turns = new List<ChatTurn>();
            foreach (var chat in recent)
            {
                if (string.IsNullOrEmpty(chat.Content)) continue;

                string plain;
                try { plain = _encryptionService.Decrypt(chat.Content); }
                catch { continue; }

                if (string.IsNullOrWhiteSpace(plain)) continue;

                if (plain.StartsWith("/bot", StringComparison.OrdinalIgnoreCase))
                {
                    plain = plain.Length > 5 ? plain.Substring(5).Trim() : string.Empty;
                    if (string.IsNullOrWhiteSpace(plain)) continue;
                }

                var role = string.Equals(chat.UserName, BotUserName, StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.Assistant
                    : ChatRole.User;

                turns.Add(new ChatTurn(role, plain));
            }

            var last = turns.Count > 0 ? turns[^1] : null;
            var alreadyTrailing = last != null
                                  && last.Role == ChatRole.User
                                  && string.Equals(last.Content, currentUserMessage, StringComparison.Ordinal);

            if (!alreadyTrailing && !string.IsNullOrWhiteSpace(currentUserMessage))
            {
                turns.Add(new ChatTurn(ChatRole.User, currentUserMessage));
            }

            return turns;
        }
    }
}
