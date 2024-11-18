using AutoMapper;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces;
using Notaion.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        public ChatService(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
        }
        public async Task<List<ChatResponseDto>> GetChatsAsync()
        {
            var chats = await _chatRepository.GetChatsAsync();
            return _mapper.Map<List<ChatResponseDto>>(chats);
        }

        public async Task<List<ChatResponseDto>> GetChatsHiddenAsync()
        {
            var chats = await _chatRepository.GetChatsHiddenAsync();
            return _mapper.Map<List<ChatResponseDto>>(chats);
        }
    }
}
