using Notaion.Domain.Entities;
using Notaion.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Interfaces
{
    public class ChatParticipantSummary
    {
        public string UserName { get; set; } = string.Empty;
        public int MessageCount { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    public interface IChatRepository : IGenericRepository<Chat>
    {
        Task<string> GetChatbotResponseAsync(string userMessage);
        Task<Chat> AddChatbotAsync(Chat chat);
        Task<List<ChatParticipantSummary>> GetParticipantsAsync();
    }
}
