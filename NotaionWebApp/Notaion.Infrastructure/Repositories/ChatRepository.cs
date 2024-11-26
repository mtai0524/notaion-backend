using Newtonsoft.Json;
using Notaion.Application.Common.Helpers;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            predefinedResponses = LoadResponsesAsync().GetAwaiter().GetResult();
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

        private Dictionary<string, List<string>> predefinedResponses;
        private async Task<Dictionary<string, List<string>>> LoadResponsesAsync()
        {
            // Xác định đường dẫn tuyệt đối đến file "responses.json" cùng thư mục với ChatRepository.cs
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "responses.json");

            if (File.Exists(filePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath); // Đọc file JSON bất đồng bộ
                    predefinedResponses = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                    return predefinedResponses ?? new Dictionary<string, List<string>>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đọc file cấu hình: {ex.Message}");
                    return new Dictionary<string, List<string>>();
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy file cấu hình câu trả lời.");
                return new Dictionary<string, List<string>>();
            }
        }

        public async Task<string> GetChatbotResponseAsync(string userMessage)
        {
            if (userMessage.StartsWith("/bot", StringComparison.OrdinalIgnoreCase))
            {
                userMessage = userMessage.Substring(5).Trim();
            }
            double similarityThreshold = 0.3;


            string bestMatchKey = null;
            double highestSimilarity = 0;

            foreach (var keyword in predefinedResponses.Keys)
            {
                int similarity = LevenshteinDistance(userMessage.ToLower(), keyword.ToLower());

                double similarityRatio = 1.0 - (double)similarity / Math.Max(userMessage.Length, keyword.Length);

                Console.WriteLine($"Similarity between '{userMessage}' and '{keyword}': {similarityRatio * 100}%");

                if (similarityRatio >= similarityThreshold && similarityRatio > highestSimilarity)
                {
                    highestSimilarity = similarityRatio;
                    bestMatchKey = keyword;
                }
            }

            if (bestMatchKey != null)
            {
                var responses = predefinedResponses[bestMatchKey];
                var randomIndex = new Random().Next(responses.Count);
                return responses[randomIndex];
            }

            return "Xin chào! Bạn cần giúp gì từ chatbot?";
        }

        public static int LevenshteinDistance(string a, string b)
        {
            int lenA = a.Length;
            int lenB = b.Length;
            int[,] distance = new int[lenA + 1, lenB + 1];

            // Khởi tạo mảng distance
            for (int i = 0; i <= lenA; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= lenB; j++)
                distance[0, j] = j;

            // Tính toán khoảng cách giữa hai chuỗi
            for (int i = 1; i <= lenA; i++)
            {
                for (int j = 1; j <= lenB; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[lenA, lenB];
        }


    }
}
