using System.Text.Json.Nodes;

namespace Notaion.Application.Services;

// Validation and shape of English items as the page sends them: a JSON object
// with "id", "createdAt" and the kind's own fields (stored as Data).
public static class EnglishItemRules
{
    public const int MaxIdLength = 64;
    public const int MaxDataLength = 200_000;
    public const int MaxBulkItems = 1000;

    // The field each kind must carry as a string; it must be non-empty unless
    // the kind allows blanks (a new writing starts empty).
    private static readonly Dictionary<string, (string Field, bool AllowEmpty)> Required = new()
    {
        ["cards"] = ("front", false),
        ["sentences"] = ("en", false),
        ["texts"] = ("body", false),
        ["writings"] = ("body", true),
    };

    public static IReadOnlyCollection<string> Kinds => Required.Keys;

    public static bool IsKind(string? kind) => kind != null && Required.ContainsKey(kind);

    // Null when the item is valid, otherwise what is wrong with it.
    public static string? Validate(string kind, JsonObject? item)
    {
        if (!IsKind(kind)) return $"Unknown kind '{kind}'. Use one of: {string.Join(", ", Kinds)}.";
        if (item == null) return "Item must be a JSON object.";

        var id = ReadString(item, "id");
        if (string.IsNullOrWhiteSpace(id)) return "Item id is required.";
        if (id.Length > MaxIdLength) return $"Item id is too long (max {MaxIdLength}).";

        var (field, allowEmpty) = Required[kind];
        var value = ReadString(item, field);
        if (value == null) return $"'{field}' is required for {kind}.";
        if (!allowEmpty && value.Trim() == string.Empty) return $"'{field}' must not be empty.";

        if (ToData(item).Length > MaxDataLength) return $"Item is too large (max {MaxDataLength} characters).";
        return null;
    }

    public static string ReadId(JsonObject item) => ReadString(item, "id") ?? string.Empty;

    public static long ReadCreatedAt(JsonObject item, long fallback)
    {
        if (item["createdAt"] is JsonValue value && value.TryGetValue(out long createdAt)) return createdAt;
        return fallback;
    }

    // The kind's own fields: everything except id and createdAt.
    public static string ToData(JsonObject item)
    {
        var data = new JsonObject();
        foreach (var (key, value) in item)
        {
            if (key is "id" or "createdAt") continue;
            data[key] = value?.DeepClone();
        }
        return data.ToJsonString();
    }

    // Back to the page's shape: { id, createdAt, ...data }.
    public static JsonObject ToItem(string id, long createdAt, string data)
    {
        var item = JsonNode.Parse(data) as JsonObject ?? new JsonObject();
        item["id"] = id;
        item["createdAt"] = createdAt;
        return item;
    }

    private static string? ReadString(JsonObject item, string field) =>
        item[field] is JsonValue value && value.TryGetValue(out string? text) ? text : null;
}
