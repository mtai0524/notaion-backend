using System.Text.Json;
using System.Text.Json.Nodes;

namespace Notaion.Application.Services;

// The daily English lesson: the AI prompt, reading the AI reply, and turning
// it into page items. Item ids are derived from the date (daily-yyyy-MM-dd-…),
// so a day's lesson is created once and re-saving it only overwrites itself.
public static class EnglishDailyLesson
{
    public const int CardCount = 10;
    public const int SentenceCount = 5;
    private const int MaxExcluded = 300;

    private static readonly string[] Topics =
    {
        "software teams", "IoT and smart devices", "job interviews", "travel", "cooking", "health and sleep",
        "money and saving", "customer support", "online shopping", "public transport", "weather and seasons",
        "sports and exercise", "movies and music", "family life", "friendship", "learning habits", "remote work",
        "meetings", "emails at work", "the environment", "city life", "hotels", "restaurants", "doctor visits",
        "news and media", "technology trends", "productivity", "small talk", "hobbies", "housing and renting",
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static string TextId(string date) => $"daily-{date}-text";

    public static string PickTopic(Random random) => Topics[random.Next(Topics.Length)];

    public static string BuildPrompt(string topic, IEnumerable<string> knownWords)
    {
        var excluded = string.Join(", ", knownWords.Take(MaxExcluded));
        return $@"You are an English teacher preparing today's lesson for a Vietnamese learner (level B1-B2).
Topic: {topic}.
Create:
- {CardCount} useful English words or short phrases (mix single words, phrasal verbs and collocations) with a Vietnamese meaning and one natural English example sentence each.
- {SentenceCount} natural English sentences people really say about the topic, each with a Vietnamese meaning and a one-word tag (work, daily, meeting, idiom, travel...).
- A reading text of 150-200 words about the topic that uses all {CardCount} words, with a short title. Separate paragraphs with \n.
- One writing task (1-2 sentences, in English) asking the learner to write 4-6 sentences about the topic.
Do NOT use any of these words or phrases the learner already has: {(excluded.Length == 0 ? "(none)" : excluded)}.
Reply with ONLY a JSON object, no markdown fences, in this exact shape:
{{""cards"": [{{""front"": """", ""back"": """", ""example"": """"}}], ""sentences"": [{{""en"": """", ""vi"": """", ""tag"": """"}}], ""text"": {{""title"": """", ""body"": """"}}, ""writingPrompt"": """"}}";
    }

    public static string Normalize(string? text) => (text ?? string.Empty).Trim().ToLowerInvariant();

    // Null when the reply has no usable lesson.
    public static LessonReply? ParseReply(string reply)
    {
        var start = reply.IndexOf('{');
        var end = reply.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            var lesson = JsonSerializer.Deserialize<LessonReply>(reply[start..(end + 1)], JsonOptions);
            if (lesson?.Text == null || string.IsNullOrWhiteSpace(lesson.Text.Body)) return null;
            return lesson;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Page items for the lesson, without cards/sentences the learner already has.
    public static Dictionary<string, List<JsonObject>> ToItems(
        LessonReply lesson, string date, long now, ISet<string> knownWords, ISet<string> knownSentences)
    {
        var cards = (lesson.Cards ?? new())
            .Where(card => !string.IsNullOrWhiteSpace(card.Front) && knownWords.Add(Normalize(card.Front)))
            .Take(CardCount)
            .Select((card, index) => new JsonObject
            {
                ["id"] = $"daily-{date}-c{index + 1}",
                ["createdAt"] = now - index,
                ["front"] = card.Front!.Trim(),
                ["back"] = card.Back?.Trim() ?? string.Empty,
                ["example"] = card.Example?.Trim() ?? string.Empty,
                ["interval"] = 0,
                ["ease"] = 2.5,
                ["due"] = now,
            })
            .ToList();

        var sentences = (lesson.Sentences ?? new())
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence.En) && knownSentences.Add(Normalize(sentence.En)))
            .Take(SentenceCount)
            .Select((sentence, index) => new JsonObject
            {
                ["id"] = $"daily-{date}-s{index + 1}",
                ["createdAt"] = now - index,
                ["en"] = sentence.En!.Trim(),
                ["vi"] = sentence.Vi?.Trim() ?? string.Empty,
                ["tag"] = sentence.Tag?.Trim() ?? string.Empty,
            })
            .ToList();

        var texts = new List<JsonObject>
        {
            new()
            {
                ["id"] = TextId(date),
                ["createdAt"] = now,
                ["title"] = string.IsNullOrWhiteSpace(lesson.Text!.Title) ? $"Daily {date}" : lesson.Text.Title.Trim(),
                ["body"] = lesson.Text.Body!.Trim(),
            },
        };

        var writings = new List<JsonObject>();
        if (!string.IsNullOrWhiteSpace(lesson.WritingPrompt))
        {
            writings.Add(new JsonObject
            {
                ["id"] = $"daily-{date}-writing",
                ["createdAt"] = now,
                ["body"] = string.Empty,
                ["prompt"] = lesson.WritingPrompt.Trim(),
                ["result"] = null,
            });
        }

        return new() { ["cards"] = cards, ["sentences"] = sentences, ["texts"] = texts, ["writings"] = writings };
    }

    public class LessonReply
    {
        public List<LessonCard>? Cards { get; set; }
        public List<LessonSentence>? Sentences { get; set; }
        public LessonText? Text { get; set; }
        public string? WritingPrompt { get; set; }
    }

    public class LessonCard
    {
        public string? Front { get; set; }
        public string? Back { get; set; }
        public string? Example { get; set; }
    }

    public class LessonSentence
    {
        public string? En { get; set; }
        public string? Vi { get; set; }
        public string? Tag { get; set; }
    }

    public class LessonText
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
    }
}
