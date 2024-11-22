using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.Common.Helpers;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Entities;
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
        private readonly IGenericRepository<Chat> _chatGenericRepository; // need use unit of work , cuz dont use Chat entity in Application layer
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public ChatService(IUnitOfWork unitOfWork, IGenericRepository<Chat> chatGenericRepository, IMapper mapper, IChatRepository chatRepository)
        {
            _chatGenericRepository = chatGenericRepository;
            _mapper = mapper;
            _chatRepository = chatRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatResponseDto> CreateChatAsync(CreateChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Content))
            {
                throw new ArgumentException("Invalid chat message.");
            }

            var chat = _mapper.Map<Chat>(chatDto);
            chat.SentDate = DateTimeHelper.GetVietnamTime();

            var createdChat = await _chatGenericRepository.AddAsync(chat);

            var response = _mapper.Map<ChatResponseDto>(createdChat);

            return response;
        }

        public async Task<int> HideChatAllAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var listChat = await _unitOfWork.ChatRepository.GetAsync(x => x.Hide == false);
                foreach (var chat in listChat)
                {
                    chat.Hide = true;
                }

                await _unitOfWork.ChatRepository.UpdateRangeAsync(listChat);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                return listChat.Count();
            }
            catch
            {
                await _unitOfWork.RollBackAsync();
                throw;
            }
        }

        /*
         * uow generic call repo not pass entity
         */
        public async Task<List<ChatResponseDto>> GetChatsAsync()
        {
            var chats = await _unitOfWork.ChatRepository.GetAsync(x => x.Hide == false);
            return _mapper.Map<List<ChatResponseDto>>(chats);
        }


        /*
         * uow generic param entity
         */
        //public async Task<List<ChatResponseDto>> GetChatsAsync()
        //{
        //    var chats = await _unitOfWork.GetGenericRepository<Chat>().GetAllAsync();
        //    return _mapper.Map<List<ChatResponseDto>>(chats);
        //}

        
        /*
         * generic repo
         */

        //public async Task<List<ChatResponseDto>> GetChatsAsync()
        //{
        //    var chats = await _chatRepository.GetAllAsync();
        //    return _mapper.Map<List<ChatResponseDto>>(chats);
        //}

        public async Task<List<ChatResponseDto>> GetChatsHiddenAsync()
        {
            var chats = await _chatGenericRepository.GetAllAsync();
            return _mapper.Map<List<ChatResponseDto>>(chats);
        }

     
    }
}
