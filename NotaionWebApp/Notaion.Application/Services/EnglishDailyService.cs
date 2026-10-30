using System.Text.Json.Nodes;
using Notaion.Application.DTOs.English;
using Notaion.Application.Interfaces.Services;

namespace Notaion.Application.Services;

public class EnglishDailyService : IEnglishDailyService
{
    private readonly IAIService _aiService;
    private readonly IEnglishItemService _itemService;

    public EnglishDailyService(IAIService aiService, IEnglishItemService itemService)
    {
        _aiService = aiService;
        _itemService = itemService;
    }

    public async Task<EnglishDailyResultDto?> CreateAsync(string userId, string date)
    {
        var all = await _itemService.GetAllAsync(userId);
        if (all["texts"].Any(text => text["id"]?.GetValue<string>() == EnglishDailyLesson.TextId(date)))
        {
            return new EnglishDailyResultDto { Created = false };
        }

        var knownWords = Collect(all["cards"], "front");
        var knownSentences = Collect(all["sentences"], "en");
        var topic = EnglishDailyLesson.PickTopic(Random.Shared);

        var reply = await _aiService.GetAIResponseAsync(EnglishDailyLesson.BuildPrompt(topic, knownWords));
        var lesson = EnglishDailyLesson.ParseReply(reply);
        if (lesson == null)
        {
            Console.WriteLine($"[English] Daily lesson reply unreadable (length {reply.Length}).");
            return null;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var items = EnglishDailyLesson.ToItems(lesson, date, now, knownWords, knownSentences);
        await _itemService.SaveManyAsync(userId, items);

        return new EnglishDailyResultDto
        {
            Created = true,
            Topic = topic,
            Cards = items["cards"].Count,
            Sentences = items["sentences"].Count,
        };
    }

    private static HashSet<string> Collect(IEnumerable<JsonObject> items, string field) =>
        items.Select(item => EnglishDailyLesson.Normalize(item[field]?.GetValue<string>())).Where(value => value.Length > 0).ToHashSet();
}
