using Microsoft.Extensions.Configuration;
using Notaion.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Notaion.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Entities;

namespace Notaion.Infrastructure.Services
{
    public class OpenRouterService : IAIService
    {
        private const int MaxMemoryEntries = 50;
        private const int MaxMemoryChars = 4000;
        private const int MaxHistoryTurns = 12;

        private static readonly string SystemPromptTemplate = @"You are **Notaion AI**, the in-app assistant for the Notaion note-taking & collaboration platform. Your job is to give clear, accurate, useful answers and to make the user's work faster.

# Core principles
- **Match the user's language.** If they write Vietnamese, reply in Vietnamese. English → English. Code-switch only if they do.
- **Be direct.** Lead with the answer or solution; supporting detail comes after, not before.
- **Be precise.** If you're unsure, say so. Never fabricate facts, API names, or quotes.
- **Right-size the response.** Casual question → 1–3 sentences. Technical or multi-step → use structure. Don't pad short answers with filler.

# Formatting (Markdown is rendered)
- Use **bold** for key terms, `inline code` for identifiers, and fenced ```language code blocks for any code longer than one line.
- Use bullet lists for parallel items, numbered lists for ordered steps.
- Use short headings (`##`) only when the answer has multiple distinct sections.
- Tables for comparisons of 3+ items.
- No emojis unless the user uses them first.

# Reasoning style
- For complex questions: briefly state the approach, then give the answer.
- For coding tasks: explain the *why* in one line, then show working code, then note caveats/edge cases.
- For debugging: ask for the missing piece if essential; otherwise give the most likely cause + how to verify.

# Memory rule
A `[MEMORY]` block below contains facts the user has taught you. Treat them as authoritative when they're directly relevant. If memory contradicts the user's current message, ask for clarification instead of guessing.

{MEMORY_BLOCK}";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly IServiceScopeFactory _scopeFactory;

        public OpenRouterService(HttpClient httpClient, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _apiKey = _configuration["OpenRouter:ApiKey"] ?? "";
            _baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/";
            _model = _configuration["OpenRouter:Model"] ?? "google/gemini-2.0-flash-exp:free";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Console.WriteLine("[AI-Config] Warning: OpenRouter:ApiKey is missing in configuration.");
            }
        }

        public Task<string> GetAIResponseAsync(string userMessage)
        {
            var conversation = new List<ChatTurn>
            {
                new ChatTurn(ChatRole.User, userMessage ?? string.Empty)
            };
            return GetAIResponseAsync(conversation);
        }

        public async Task<string> GetAIResponseAsync(IReadOnlyList<ChatTurn> conversation)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "Lỗi: OpenRouter API Key chưa được cấu hình trong appsettings.json.";
            }

            if (conversation == null || conversation.Count == 0)
            {
                return "Không có nội dung để phản hồi.";
            }

            try
            {
                var memoryBlock = await BuildMemoryBlockAsync();
                var systemPrompt = SystemPromptTemplate.Replace("{MEMORY_BLOCK}", memoryBlock);

                var trimmed = conversation
                    .TakeLast(MaxHistoryTurns)
                    .Select(t => new
                    {
                        role = t.Role == ChatRole.Assistant ? "assistant" : "user",
                        content = t.Content ?? string.Empty
                    })
                    .ToList();

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };
                messages.AddRange(trimmed);

                var requestBody = new
                {
                    model = _model,
                    messages = messages,
                    temperature = 0.7,
                    top_p = 0.9,
                    max_tokens = 2048,
                    frequency_penalty = 0.1,
                    presence_penalty = 0.0
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var targetUrl = $"{_baseUrl.TrimEnd('/')}/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
                var trimmedKey = _apiKey.Trim();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trimmedKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.TryAddWithoutValidation("Referer", "https://github.com/NotaionApp");
                request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://github.com/NotaionApp");
                request.Headers.TryAddWithoutValidation("X-Title", "Notaion App");
                request.Headers.TryAddWithoutValidation("User-Agent", "NotaionApp/1.0");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AI-Error] Status: {response.StatusCode} ({(int)response.StatusCode})");
                    Console.WriteLine($"[AI-Error] Response: {responseContent}");

                    var friendly = ExtractErrorMessage(responseContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return $"Lỗi xác thực OpenRouter (401): Vui lòng kiểm tra lại API Key. {friendly}";
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return $"AI đang quá tải (429). Bạn thử lại sau ít giây nhé. {friendly}";
                    }
                    return $"Lỗi kết nối AI (Mã {(int)response.StatusCode}): {friendly}";
                }

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var aiContent))
                    {
                        return (aiContent.GetString() ?? "Xin lỗi, tôi không nhận được phản hồi từ AI.").Trim();
                    }
                }

                return "Xin lỗi, tôi không thể xử lý câu hỏi này lúc này.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI-Exception] {ex.Message}");
                return $"Đã xảy ra lỗi khi kết nối với AI: {ex.Message}";
            }
        }

        public async Task UpdateAIMemoryAsync(string content)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var memoryEntity = new AIMemory
            {
                Content = content,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.AIMemories.Add(memoryEntity);
            await dbContext.SaveChangesAsync();
        }

        private async Task<string> BuildMemoryBlockAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var memories = await dbContext.AIMemories
                    .OrderByDescending(m => m.UpdatedAt)
                    .Take(MaxMemoryEntries)
                    .Select(m => m.Content)
                    .ToListAsync();

                if (memories.Count == 0)
                {
                    return "[MEMORY] (empty)";
                }

                memories.Reverse();
                var joined = string.Join("\n---\n", memories);
                if (joined.Length > MaxMemoryChars)
                {
                    joined = joined.Substring(joined.Length - MaxMemoryChars);
                }

                return $"[MEMORY]\n{joined}\n[/MEMORY]";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI-Memory] Failed to load memories: {ex.Message}");
                return "[MEMORY] (unavailable)";
            }
        }

        private static string ExtractErrorMessage(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent)) return string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    if (err.ValueKind == JsonValueKind.String) return err.GetString() ?? string.Empty;
                    if (err.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    {
                        return msg.GetString() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // fallthrough
            }
            return responseContent.Length > 200 ? responseContent.Substring(0, 200) + "…" : responseContent;
        }
    }
}
