using Notaion.Application.DTOs;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Options;

namespace Notaion.Application.Interfaces.Services
{
    public interface IChatService
    {
        Task<PaginatedResultDto<ChatResponseDto>> GetChatsAsync(QueryOptions options, bool decypt = false);
        Task<List<ChatResponseDto>> GetChatsHiddenAsync(bool decrypt = false);
        Task<ChatResponseDto> CreateChatAsync(CreateChatDto chatDto);
        Task<ChatResponseDto> CreateChatbotAsync(CreateChatDto chatDto);
        Task<int> HideChatAllAsync();
    }
}
