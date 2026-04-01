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
            _apiKey = _configuration["OpenRouter:ApiKey"] ?? throw new ArgumentNullException("OpenRouter:ApiKey is missing");
            _baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/";
            _model = _configuration["OpenRouter:Model"] ?? "google/gemini-2.0-flash-exp:free";
        }

        public async Task<string> GetAIResponseAsync(string userMessage)
        {
            try
            {
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là một trợ lý ảo thông minh tên là Notion AI. Hãy trả lời câu hỏi của người dùng một cách thân thiện và chính xác." },
                        new { role = "user", content = userMessage }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Chuẩn hóa URL một cách an toàn
                var targetUrl = $"{_baseUrl.TrimEnd('/')}/chat/completions";

                // Sử dụng PostAsync trực tiếp
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/NotaionApp");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "Notaion App");

                var response = await _httpClient.PostAsync(targetUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"Lỗi kết nối AI (Mã {response.StatusCode}): {errorDetails}";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var aiContent))
                    {
                        var result = aiContent.GetString() ?? "Xin lỗi, tôi không nhận được phản hồi từ AI.";
                        return result.Trim(); // Làm sạch kết quả
                    }
                }

                return "Xin lỗi, tôi không thể xử lý câu hỏi này lúc này.";
            }
            catch (Exception ex)
            {
                return $"Đã xảy ra lỗi khi kết nối với AI: {ex.Message}";
            }
        }
    }
}
