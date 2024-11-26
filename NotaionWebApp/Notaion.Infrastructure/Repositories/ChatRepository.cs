using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.Common.Helpers;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Services;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Infrastructure.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Chat> AddChatbotAsync(Chat chat)
        {
            var botResponse = await GetChatbotResponseAsync(chat.Content);

            var botChat = new Chat
            {
                Content = botResponse,
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
            var predefinedResponses = new Dictionary<string, string>
            {
                { "hello", "Chào bạn! Tôi là chatbot." },
                { "xin chào", "Chào bạn! Tôi là chatbot." },
                { "hi", "Chào bạn! Tôi là chatbot." },
                { "how are you", "Tôi là chatbot, luôn sẵn sàng hỗ trợ bạn." },
                { "how's it going", "Tôi là chatbot, luôn sẵn sàng hỗ trợ bạn." },
                { "bye", "Tạm biệt! Hẹn gặp lại." },
                { "goodbye", "Tạm biệt! Hẹn gặp lại." },
                { "what is your name", "Tôi là một chatbot không có tên riêng." },
                { "can you help me", "Tất nhiên! Bạn cần giúp đỡ về vấn đề gì?" },
                { "how do I get to the nearest coffee shop", "Bạn có thể tìm thấy quán cà phê gần nhất bằng cách sử dụng Google Maps." },
                { "tell me a joke", "Tại sao máy tính không bao giờ đói? Vì nó đã có đủ bộ nhớ!" },
                { "what time is it", "Sorry, I can't provide the time information." },
                { "what is the weather like", "I'm sorry, I can't provide current weather information." },
                { "who are you", "Tôi là một chatbot được lập trình để hỗ trợ bạn." },
                { "thank you", "You're welcome! I'm here to help!" },
                { "sorry", "Không sao, bạn cần thêm sự trợ giúp nào không?" },
                { "good morning", "Good morning! Have a great day ahead." },
                { "good night", "Chúc bạn ngủ ngon! Hẹn gặp lại vào ngày mai." },
                { "how old are you", "Tôi không có tuổi, tôi chỉ là một chương trình." },
                { "what can you do", "I can help you with answering questions, sending messages, and much more." },
                { "tell me a fact", "Did you know that the Earth rotates around the Sun at a speed of about 30 km/s?" },
                { "are you real", "Tôi không phải là con người, nhưng tôi có thể giúp bạn rất nhiều điều." },
                { "what is your favorite color", "Màu sắc yêu thích của tôi là màu xanh dương." },
                { "info", "Xin chào tôi là chatbot do minhtai training, mục đích dùng để sau khi cậu chủ không còn nữa thì tôi vẫn có thể thay thế :>" },
                { "help",
                    "Dưới đây là một số lệnh mà bạn có thể sử dụng:\n" +
                    "/shortcut - Hiển thị các phím tắt\n" +
                    "/hello - Chào chatbot\n" +
                    "/bye - Tạm biệt chatbot\n" +
                    "/joke - Xem một câu chuyện vui\n" +
                    "/fact - Nhận một thông tin thú vị" +
                    "/info - Xem thông tin chatbot"
                },
              { "are you here", "Tôi vẫn luôn ở đây, mãi mãi không rời đi :>" },
              { "bạn có đó không", "Tôi vẫn luôn ở đây, mãi mãi không rời đi :>" },
              { "/shortcut", "alt + x : toggle drawer \n" + "alt + c : toggle chat box" },

            };
            foreach (var keyword in predefinedResponses.Keys)
            {
                if (userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return predefinedResponses[keyword];
                }
            }

            return "Xin chào! Bạn cần giúp gì từ chatbot?";
        }
    }
}
