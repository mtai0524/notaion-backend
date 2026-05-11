using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Domain.Entities
{
    [Table("PageVisit")]
    public class PageVisit
    {
        [Key]
        public int Id { get; set; }
        public string? Path { get; set; }
        public bool IsLocalhost { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
