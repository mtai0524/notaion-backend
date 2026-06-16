using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Domain.Entities
{
    // A single turn in a user's private 1-on-1 AI assistant conversation.
    // Each account owns its own thread (filtered by UserId); Content is stored
    // encrypted at rest, like the other chat tables.
    [Table("AiChat")]
    public class AiChat
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        // "user" | "assistant"
        public string Role { get; set; } = "user";

        // Encrypted (AES) message text.
        public string? Content { get; set; }

        public DateTime SentDate { get; set; }
    }
}
