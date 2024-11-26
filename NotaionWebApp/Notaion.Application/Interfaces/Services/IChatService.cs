using Notaion.Application.DTOs.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Interfaces.Services
{
    public interface IChatService
    {
        Task<List<ChatResponseDto>> GetChatsAsync();
        Task<List<ChatResponseDto>> GetChatsHiddenAsync();
        Task<ChatResponseDto> CreateChatAsync(CreateChatDto chatDto);
        Task<ChatResponseDto> CreateChatbotAsync(CreateChatDto chatDto);
        Task<int> HideChatAllAsync();
    }
}
