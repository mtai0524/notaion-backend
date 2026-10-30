using System.Text.Json.Nodes;

namespace Notaion.Application.Interfaces.Services
{
    // A user's English practice data. Items are passed in the page's shape
    // ({ id, createdAt, ...fields }) and must be validated by the caller
    // (EnglishItemRules.Validate).
    public interface IEnglishItemService
    {
        // Every kind is present, newest first.
        Task<Dictionary<string, List<JsonObject>>> GetAllAsync(string userId);

        Task<JsonObject> SaveAsync(string userId, string kind, JsonObject item);

        // Returns how many items were saved.
        Task<int> SaveManyAsync(string userId, Dictionary<string, List<JsonObject>> itemsByKind);

        // False when the item does not exist.
        Task<bool> DeleteAsync(string userId, string kind, string id);
    }
}
