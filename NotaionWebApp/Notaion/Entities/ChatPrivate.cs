using Notaion.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Notaion.Entities
{
    [Table("ChatPrivate")]
    public class ChatPrivate
    {
        [Key]
        public string Id { get; set; }
        public string? Content { get; set; }
        public DateTime? SentDate { get; set; }
        public string? Sender { get; set; }
        [ForeignKey("UserId")]
        public string? Receiver { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public bool Hide { get; set; } = false;
        public bool IsNew { get; set; } = false;
    }
}
