using Notaion.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Entities
{
    [Table("Page")]
    public class Page
    {
        [Key]
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? Public { get; set; }
    }
}
