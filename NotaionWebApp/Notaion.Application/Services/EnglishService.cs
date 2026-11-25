using System.Text.Json;
using Notaion.Application.DTOs.English;
using Notaion.Application.Interfaces.Services;

namespace Notaion.Application.Services;

// English writing check and word translation. Stateless: nothing is stored
// and the user's AI chat thread is not touched.
public class EnglishService : IEnglishService
{
    private const string CheckPrompt = @"You are an English writing teacher for a Vietnamese learner.
Correct the text below. Keep the writer's meaning and voice; fix grammar, word choice, spelling and punctuation only.
Reply with ONLY a JSON object, no markdown fences, in this exact shape:
{""corrected"": ""<the full corrected text>"", ""notes"": [""<short explanation of one mistake, in English>""], ""score"": <integer 1-10>}
If the text has no mistakes, return it unchanged with an empty notes array.

Text:
";

    private const string TranslatePrompt = @"You are an English-Vietnamese dictionary for a Vietnamese learner.
Give the Vietnamese meaning of the English word or phrase below. If a sentence is given, give the sense the word has in that sentence.
Reply with ONLY a JSON object, no markdown fences, in this exact shape:
{""meaning"": ""(<part of speech: n. / v. / adj. / adv. / phrase>) <1 to 3 short Vietnamese meanings, comma-separated>""}
Example: {""meaning"": ""(adj.) kiên cường, mau phục hồi""}

";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IAIService _aiService;

    public EnglishService(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<EnglishCheckResultDto?> CheckAsync(string text)
    {
        var reply = await _aiService.GetAIResponseAsync(CheckPrompt + text);
        return ParseResult(reply);
    }

    public async Task<EnglishTranslateResultDto?> TranslateAsync(string word, string? context)
    {
        var prompt = $"{TranslatePrompt}Word: {word}";
        if (!string.IsNullOrWhiteSpace(context)) prompt += $"\nSentence: {context}";

        var reply = await _aiService.GetAIResponseAsync(prompt);
        return ParseMeaning(reply);
    }

    // The model sometimes wraps the JSON in prose or fences; take the outer object.
    private static string? OuterJson(string reply)
    {
        var start = reply.IndexOf('{');
        var end = reply.LastIndexOf('}');
        return start >= 0 && end > start ? reply[start..(end + 1)] : null;
    }

    private static EnglishCheckResultDto? ParseResult(string reply)
    {
        var json = OuterJson(reply);
        if (json == null)
        {
            Console.WriteLine($"[English] AI reply has no JSON object (length {reply.Length}).");
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize<EnglishCheckResultDto>(json, JsonOptions);
            if (result == null || string.IsNullOrWhiteSpace(result.Corrected))
            {
                Console.WriteLine("[English] AI reply JSON has no corrected text.");
                return null;
            }
            result.Score = Math.Clamp(result.Score, 1, 10);
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[English] Invalid JSON from AI: {ex.Message}");
            return null;
        }
    }

    private static EnglishTranslateResultDto? ParseMeaning(string reply)
    {
        var json = OuterJson(reply);
        if (json == null)
        {
            Console.WriteLine($"[English] Translation reply has no JSON object (length {reply.Length}).");
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize<EnglishTranslateResultDto>(json, JsonOptions);
            if (result == null || string.IsNullOrWhiteSpace(result.Meaning))
            {
                Console.WriteLine("[English] Translation reply JSON has no meaning.");
                return null;
            }
            result.Meaning = result.Meaning.Trim();
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[English] Invalid translation JSON from AI: {ex.Message}");
            return null;
        }
    }
}
