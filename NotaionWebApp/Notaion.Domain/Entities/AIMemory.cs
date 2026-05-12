using System.ComponentModel.DataAnnotations;

namespace Notaion.Domain.Entities
{
    public class AIMemory
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; } // Memory can be global or per-user
    }
}
