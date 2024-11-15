using Notaion.Domain.Common;
using Notaion.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Domain.Entities
{
    [Table("Chat")]
    public class Chat : BaseEntity<Guid>
    {
        public string? Content { get; set; }
        public DateTime? SentDate { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public string? UserName { get; set; }
        public bool Hide { get; set; } = false;
    }
}
