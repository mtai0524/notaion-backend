using Notaion.Application.DTOs.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatResponseDto>> GetChatsAsync();
        Task<List<ChatResponseDto>> GetChatsHiddenAsync();
    }
}
