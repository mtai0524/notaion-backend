using System.ComponentModel.DataAnnotations;

namespace Notaion.Domain.Entities
{
    // One item of a user's English practice data (/english page): a card,
    // sentence, text or writing. Each kind's fields live in Data (JSON) so the
    // page can grow a field without a migration. Key: (UserId, Id).
    public class EnglishItem
    {
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Client-generated id, unique per user.
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        // "cards" | "sentences" | "texts" | "writings"
        [MaxLength(16)]
        public string Kind { get; set; } = string.Empty;

        public string Data { get; set; } = "{}";

        // Unix milliseconds, as the page uses them.
        public long CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
