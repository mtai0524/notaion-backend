using Microsoft.Extensions.Configuration;
using Notaion.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Notaion.Infrastructure.Services
{
    public class OpenRouterService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;

        public OpenRouterService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["OpenRouter:ApiKey"] ?? "";
            _baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/";
            _model = _configuration["OpenRouter:Model"] ?? "google/gemini-2.0-flash-exp:free";
            
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Console.WriteLine("[AI-Config] Warning: OpenRouter:ApiKey is missing in configuration.");
            }
        }

        public async Task<string> GetAIResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "Lỗi: OpenRouter API Key chưa được cấu hình trong appsettings.json.";
            }

            try
            {
                // Đọc bộ nhớ từ file (Context)
                string aiMemory = "";
                
                string memoryPath = Path.Combine(AppContext.BaseDirectory, "ai_memory.txt");
                if (File.Exists(memoryPath))
                {
                    aiMemory = await File.ReadAllTextAsync(memoryPath);
                }

                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { 
                            role = "system", 
                            content = $@"Bạn là Notion AI - một trợ lý thông minh. 
                            QUY TẮC QUAN TRỌNG: Bạn có một bộ nhớ cá nhân dưới đây. Nếu câu hỏi của người dùng liên quan đến thông tin trong bộ nhớ này, bạn PHẢI ưu tiên sử dụng nó để trả lời.

                            [BỘ NHỚ CÁ NHÂN]:
                            {aiMemory}
                            [KẾT THÚC BỘ NHỚ]

                            Hãy trả lời một cách tự nhiên, thân thiện và chính xác dựa trên kiến thức trên." 
                        },
                        new { role = "user", content = userMessage }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Chuẩn hóa URL một cách an toàn
                var targetUrl = $"{_baseUrl.TrimEnd('/')}/chat/completions";

                // Sử dụng HttpRequestMessage để đảm bảo thread-safe và tránh lỗi header khi chạy song song
                using (var request = new HttpRequestMessage(HttpMethod.Post, targetUrl))
                {
                    var trimmedKey = _apiKey.Trim();
                    
                    // Diagnostic logging
                    Console.WriteLine($"[AI-Debug] Target URL: {targetUrl}");
                    Console.WriteLine($"[AI-Debug] Using API Key (length: {trimmedKey.Length}) starting with: {trimmedKey.Substring(0, Math.Min(8, trimmedKey.Length))}...");

                    // Thiết lập Authorization header
                    // Một số môi trường có thể gặp vấn đề với AuthenticationHeaderValue, thử dùng TryAddWithoutValidation nếu cần
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trimmedKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    // Các header đặc thù của OpenRouter
                    // Sử dụng cả Referer và HTTP-Referer để đảm bảo tương thích tối đa
                    request.Headers.TryAddWithoutValidation("Referer", "https://github.com/NotaionApp");
                    request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://github.com/NotaionApp");
                    request.Headers.TryAddWithoutValidation("X-Title", "Notaion App");
                    request.Headers.TryAddWithoutValidation("User-Agent", "NotaionApp/1.0");
                    
                    request.Content = content;

                    var response = await _httpClient.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        // OpenRouter trả về JSON chi tiết lỗi, chúng ta cần log lại để debug
                        Console.WriteLine($"[AI-Error] Status: {response.StatusCode} ({(int)response.StatusCode})");
                        Console.WriteLine($"[AI-Error] Response: {responseContent}");
                        
                        // Kiểm tra nếu là lỗi 401 Unauthorized
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return $"Lỗi xác thực OpenRouter (401): Vui lòng kiểm tra lại API Key trong appsettings.json. Chi tiết: {responseContent}";
                        }
                        
                        return $"Lỗi kết nối AI (Mã {response.StatusCode}): {responseContent}";
                    }

                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var aiContent))
                        {
                            var result = aiContent.GetString() ?? "Xin lỗi, tôi không nhận được phản hồi từ AI.";
                            return result.Trim();
                        }
                    }

                    return "Xin lỗi, tôi không thể xử lý câu hỏi này lúc này.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI-Exception] {ex.Message}");
                return $"Đã xảy ra lỗi khi kết nối với AI: {ex.Message}";
            }
        }
    }
}
