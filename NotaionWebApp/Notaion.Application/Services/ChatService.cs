using AutoMapper;
using Notaion.Application.Common.Helpers;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using System.Security.Cryptography;

namespace Notaion.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IGenericRepository<Chat> _chatGenericRepository; // need use unit of work , cuz dont use Chat entity in Application layer
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        public ChatService(IUnitOfWork unitOfWork, IGenericRepository<Chat> chatGenericRepository, IMapper mapper, IChatRepository chatRepository, IEncryptionService encryptionService)
        {
            _chatGenericRepository = chatGenericRepository;
            _mapper = mapper;
            _chatRepository = chatRepository;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
        }

        public async Task<ChatResponseDto> CreateChatbotAsync(CreateChatDto chatDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                chatDto.Content = _encryptionService.Encrypt(chatDto.Content);

                var chat = _mapper.Map<Chat>(chatDto);

                var createdChatbot = await _unitOfWork.ChatRepository.AddChatbotAsync(chat);

                await _unitOfWork.SaveChangeAsync();

                var response = _mapper.Map<ChatResponseDto>(createdChatbot);

                await _unitOfWork.CommitTransactionAsync(); // không có commit thì hủy tất cả thay đổi (sau này làm chat ẩn danh)

                return response;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ChatResponseDto> CreateChatAsync(CreateChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Content))
            {
                throw new ArgumentException("Invalid chat message.");
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                chatDto.Content = _encryptionService.Encrypt(chatDto.Content);

                var chat = _mapper.Map<Chat>(chatDto);
                chat.SentDate = DateTimeHelper.GetVietnamTime();

                var createdChat = await _unitOfWork.ChatRepository.AddAsync(chat);

                await _unitOfWork.SaveChangeAsync();

                var response = _mapper.Map<ChatResponseDto>(createdChat);

                await _unitOfWork.CommitTransactionAsync();

                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /*
         * uow generic call repo not pass entity
         */
        public async Task<List<ChatResponseDto>> GetChatsAsync()
        {
            var chats = await _unitOfWork.ChatRepository.GetAsync(x => x.Hide == false);

            var chatResponseDtos = new List<ChatResponseDto>();

            foreach (var chat in chats)
            {
                var chatDto = _mapper.Map<ChatResponseDto>(chat);

                try
                {
                    var encryptionService = new EncryptionService();

                    string plainText = "Hello, world!";
                    string encrypted = encryptionService.Encrypt(plainText);
                    string decrypted = encryptionService.Decrypt(encrypted);

                    Console.WriteLine($"PlainText: {plainText}");
                    Console.WriteLine($"Encrypted: {encrypted}");
                    Console.WriteLine($"Decrypted: {decrypted}");

                    // giải mã
                    chatDto.Content = _encryptionService.Decrypt(chat.Content);
                }
                catch (CryptographicException ex)
                {
                    Console.WriteLine($"Failed to decrypt chat content. Chat ID: {chat.Id}, Error: {ex.Message}");
                    chatDto.Content = "[Unable to decrypt message]";
                }

                chatResponseDtos.Add(chatDto);
            }

            return chatResponseDtos;
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
